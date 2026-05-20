using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace OrderService.Infrastructure;

public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=order;Username=postgres;Password=postgres");
        return new OrderDbContext(optionsBuilder.Options, new NullCurrentTenantService(), null);
    }

    private sealed class NullCurrentTenantService : ICurrentTenantService
    {
        public TenantId TenantId => TenantId.Empty;
    }
}
