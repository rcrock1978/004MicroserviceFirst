using SaaSCommon.Domain;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain;

namespace OrderService.Application.Tests;

public class TestOrderDbContext : DbContext, IOrderDbContext
{
    public DbSet<Order> Orders { get; set; } = null!;

    public TestOrderDbContext(DbContextOptions<TestOrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.CustomerId);
            entity.Property(e => e.Status);
            entity.Property(e => e.TenantId);
            entity.Ignore(e => e.Items);
            entity.Ignore(e => e.TotalAmount);
        });
    }
}
