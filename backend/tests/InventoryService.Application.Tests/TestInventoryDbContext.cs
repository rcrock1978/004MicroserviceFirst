using SaaSCommon.Domain;
using Microsoft.EntityFrameworkCore;
using InventoryService.Application;
using InventoryService.Domain;

namespace InventoryService.Application.Tests;

public class TestInventoryDbContext : DbContext, IInventoryDbContext
{
    public DbSet<StockItem> StockItems { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    public TestInventoryDbContext(DbContextOptions<TestInventoryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ProductId);
            entity.Property(e => e.QuantityAvailable);
            entity.Property(e => e.QuantityReserved);
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.OrderId);
            entity.Property(e => e.StockItemId);
            entity.Property(e => e.Quantity);
            entity.Property(e => e.ExpiresAt);
            entity.Property(e => e.Status);
        });
    }
}
