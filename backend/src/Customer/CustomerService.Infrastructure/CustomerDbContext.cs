using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.EntityFrameworkCore;
using SaaSCommon.Infrastructure.Tenancy;
using CustomerService.Application;
using CustomerService.Domain;

namespace CustomerService.Infrastructure;

public sealed class CustomerDbContext : DbContext, ICustomerDbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    public CustomerDbContext(
        DbContextOptions<CustomerDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerOrderHistory> CustomerOrderHistory => Set<CustomerOrderHistory>();

    private TenantId CurrentTenantId => _currentTenantService.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.ApplyConfiguration(new EntityConfigurations.CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new EntityConfigurations.CustomerOrderHistoryConfiguration());

        modelBuilder.AddEfCoreTenantFilter<Customer>(CurrentTenantId);
        modelBuilder.Entity<CustomerOrderHistory>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddInboxStateEntity();
    }
}
