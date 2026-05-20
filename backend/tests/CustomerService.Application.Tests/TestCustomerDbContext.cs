using SaaSCommon.Domain;
using Microsoft.EntityFrameworkCore;
using CustomerService.Application;
using CustomerService.Domain;

namespace CustomerService.Application.Tests;

public class TestCustomerDbContext : DbContext, ICustomerDbContext
{
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<CustomerOrderHistory> CustomerOrderHistory { get; set; } = null!;

    public TestCustomerDbContext(DbContextOptions<TestCustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<SaaSCommon.Domain.DomainEvent>();
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.Email);
            entity.Property(e => e.Name);
            entity.Property(e => e.Phone);
            entity.Property(e => e.TenantId);
        });

        modelBuilder.Entity<CustomerOrderHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TenantId).HasConversion(v => v.Value, v => new TenantId(v));
            entity.Property(e => e.CustomerId);
            entity.Property(e => e.OrderId);
            entity.Property(e => e.TenantId);
        });
    }
}
