using SaaSCommon.Domain;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application;
using PaymentService.Domain;

namespace PaymentService.Application.Tests;

public class TestPaymentDbContext : DbContext, IPaymentDbContext
{
    public DbSet<Payment> Payments { get; set; } = null!;

    public TestPaymentDbContext(DbContextOptions<TestPaymentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.OrderId);
            entity.Property(e => e.Amount);
            entity.Property(e => e.Status);
            entity.Property(e => e.TenantId);
        });
    }
}
