using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace CustomerService.Infrastructure;

public sealed class CustomerDbContextFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomerDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=customer;Username=postgres;Password=postgres");

        return new CustomerDbContext(optionsBuilder.Options, new DummyTenantService());
    }

    private sealed class DummyTenantService : ICurrentTenantService
    {
        public TenantId TenantId => TenantId.Empty;
    }
}
