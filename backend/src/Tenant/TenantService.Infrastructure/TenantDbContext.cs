using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.EntityFrameworkCore;
using SaaSCommon.Infrastructure.Tenancy;
using TenantService.Application;
using TenantService.Domain;

namespace TenantService.Infrastructure;

public sealed class TenantDbContext : DbContext, ITenantDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublishEndpoint? _publishEndpoint;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ICurrentTenantService currentTenantService,
        IPublishEndpoint? publishEndpoint = null)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    private TenantId CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.ApplyConfiguration(new EntityConfigurations.TenantConfiguration());

        modelBuilder.AddEfCoreTenantFilter<Tenant>(CurrentTenantId);

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            if (_publishEndpoint is not null)
            {
                foreach (var domainEvent in events)
                {
                    var integrationEvent = MapDomainEventToIntegrationEvent(domainEvent);
                    if (integrationEvent is not null)
                    {
                        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static object? MapDomainEventToIntegrationEvent(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            TenantProvisioned e => new Contracts.TenantProvisionedEvent(e.TenantId, e.Name, e.Slug, e.OccurredOn),
            TenantActivated e => new Contracts.TenantActivatedEvent(e.TenantId, e.OccurredOn),
            TenantDeactivated e => new Contracts.TenantDeactivatedEvent(e.TenantId, e.OccurredOn),
            TenantConfigurationUpdated e => new Contracts.TenantConfigurationUpdatedEvent(
                e.TenantId,
                e.Configuration.Settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                e.OccurredOn),
            FeatureFlagToggled e => new Contracts.FeatureFlagToggledEvent(e.TenantId, e.Key, e.Enabled, e.OccurredOn),
            _ => null
        };
    }
}
