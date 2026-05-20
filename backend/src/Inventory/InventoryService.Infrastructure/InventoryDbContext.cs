using MassTransit;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.EntityFrameworkCore;
using SaaSCommon.Infrastructure.Tenancy;
using InventoryService.Application;
using InventoryService.Domain;

namespace InventoryService.Infrastructure;

public sealed class InventoryDbContext : DbContext, IInventoryDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPublishEndpoint? _publishEndpoint;

    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
        ICurrentTenantService currentTenantService,
        IPublishEndpoint? publishEndpoint = null)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    private TenantId CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.StockItemConfiguration());
        modelBuilder.ApplyConfiguration(new Persistence.Configurations.ReservationConfiguration());

        modelBuilder.AddEfCoreTenantFilter<StockItem>(CurrentTenantId);
        modelBuilder.AddEfCoreTenantFilter<Reservation>(CurrentTenantId);

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
            StockReserved e => new Contracts.StockReservedEvent(e.StockItemId, e.ProductId, e.TenantId.Value, e.OrderId, e.Quantity, e.ExpiresAt, e.OccurredOn),
            StockReservationReleased e => new Contracts.StockReservationReleasedEvent(e.StockItemId, e.ProductId, e.TenantId.Value, e.OrderId, e.Quantity, e.OccurredOn),
            StockReservationExpired e => new Contracts.StockReservationExpiredEvent(e.StockItemId, e.ProductId, e.TenantId.Value, e.OrderId, e.Quantity, e.OccurredOn),
            StockAdjusted e => new Contracts.StockAdjustedEvent(e.StockItemId, e.ProductId, e.TenantId.Value, e.Delta, e.NewAvailable, e.OccurredOn),
            _ => null
        };
    }
}
