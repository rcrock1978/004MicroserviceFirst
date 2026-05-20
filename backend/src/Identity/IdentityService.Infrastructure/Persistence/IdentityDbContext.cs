using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;
using IdentityService.Application;
using IdentityService.Domain;
using IdentityService.Contracts.IntegrationEvents;

namespace IdentityService.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IIdentityDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TenantId _currentTenantId;

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ICurrentTenantService currentTenantService,
        IPublishEndpoint publishEndpoint)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _publishEndpoint = publishEndpoint;
        _currentTenantId = currentTenantService?.TenantId ?? TenantId.Empty;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.ApplyConfiguration(new Configurations.UserProfileConfiguration());
        modelBuilder.Entity<UserProfile>().HasQueryFilter(e => e.TenantId == _currentTenantId);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        if (_publishEndpoint is not null)
        {
            foreach (var domainEvent in domainEvents)
            {
                var integrationEvent = MapToIntegrationEvent(domainEvent);
                if (integrationEvent is not null)
                {
                    await _publishEndpoint.Publish(integrationEvent, cancellationToken);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static object? MapToIntegrationEvent(DomainEvent domainEvent) => domainEvent switch
    {
        UserProfileCreated e => new UserProfileCreatedEvent(
            e.UserProfileId, e.TenantId.Value, e.ExternalId, e.Email, e.DisplayName,
            e.Roles, e.Claims, e.OccurredOn),
        UserProfileUpdated e => new UserProfileUpdatedEvent(
            e.UserProfileId, e.TenantId.Value, e.ExternalId, e.Email, e.DisplayName,
            e.Roles, e.Claims, e.OccurredOn),
        UserProfileDeactivated e => new UserProfileDeactivatedEvent(
            e.UserProfileId, e.TenantId.Value, e.ExternalId, e.OccurredOn),
        _ => null
    };
}
