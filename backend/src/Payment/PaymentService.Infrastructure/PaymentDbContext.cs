using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.EntityFrameworkCore;
using SaaSCommon.Infrastructure.Tenancy;
using PaymentService.Application;
using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public sealed class PaymentDbContext : DbContext, IPaymentDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublishEndpoint? _publishEndpoint;

    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options,
        ICurrentTenantService currentTenantService,
        IPublishEndpoint? publishEndpoint = null)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<Payment> Payments => Set<Payment>();

    private TenantId CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.PaymentConfiguration());

        modelBuilder.AddEfCoreTenantFilter<Payment>(CurrentTenantId);

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => new { Entity = e.Entity, Events = e.Entity.DomainEvents.ToList() })
            .ToList();

        foreach (var item in entitiesWithEvents)
        {
            item.Entity.ClearDomainEvents();
        }

        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_publishEndpoint is not null)
        {
            foreach (var item in entitiesWithEvents)
            {
                foreach (var domainEvent in item.Events)
                {
                    var integrationEvent = MapDomainEventToIntegrationEvent(domainEvent);
                    if (integrationEvent is not null)
                    {
                        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
                    }
                }
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private static object? MapDomainEventToIntegrationEvent(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            PaymentProcessed e => new Contracts.PaymentProcessedEvent(e.PaymentId, e.OrderId, e.TenantId.Value, e.Amount, e.ProviderReference, e.OccurredOn),
            PaymentFailed e => new Contracts.PaymentFailedEvent(e.PaymentId, e.OrderId, e.TenantId.Value, e.Amount, e.Reason, e.OccurredOn),
            PaymentRefunded e => new Contracts.PaymentRefundedEvent(e.PaymentId, e.OrderId, e.TenantId.Value, e.Amount, e.ProviderReference, e.OccurredOn),
            _ => null
        };
    }
}
