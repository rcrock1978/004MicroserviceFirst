using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.EntityFrameworkCore;
using SaaSCommon.Infrastructure.Tenancy;
using OrderService.Application;
using OrderService.Domain;

namespace OrderService.Infrastructure;

public sealed class OrderDbContext : DbContext, IOrderDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublishEndpoint? _publishEndpoint;

    public OrderDbContext(
        DbContextOptions<OrderDbContext> options,
        ICurrentTenantService currentTenantService,
        IPublishEndpoint? publishEndpoint = null)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<Order> Orders => Set<Order>();

    private TenantId CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.OrderConfiguration());

        modelBuilder.AddEfCoreTenantFilter<Order>(CurrentTenantId);

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
            OrderCreated e => new Contracts.OrderCreatedEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.OccurredOn),
            OrderPlaced e => new Contracts.OrderPlacedEvent(
                e.OrderId, e.CustomerId, e.TenantId.Value,
                e.Items.Select(i => new Contracts.OrderItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.LineTotal)).ToList(),
                e.TotalAmount, e.OccurredOn),
            OrderPaymentRequested e => new Contracts.OrderPaymentRequestedEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.Amount, e.OccurredOn),
            OrderPaid e => new Contracts.OrderPaidEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.Amount, e.ProviderReference, e.OccurredOn),
            OrderPaymentFailed e => new Contracts.OrderPaymentFailedEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.Amount, e.OccurredOn),
            OrderShipped e => new Contracts.OrderShippedEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.OccurredOn),
            OrderCompleted e => new Contracts.OrderCompletedEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.OccurredOn),
            OrderCancelled e => new Contracts.OrderCancelledEvent(e.OrderId, e.CustomerId, e.TenantId.Value, e.OccurredOn),
            _ => null
        };
    }
}
