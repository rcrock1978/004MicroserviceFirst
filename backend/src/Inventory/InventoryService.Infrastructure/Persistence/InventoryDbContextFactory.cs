using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace InventoryService.Infrastructure;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=inventory;Username=postgres;Password=postgres");
        return new InventoryDbContext(optionsBuilder.Options, new NullCurrentTenantService(), null);
    }

    private sealed class NullCurrentTenantService : ICurrentTenantService
    {
        public TenantId TenantId => TenantId.Empty;
    }
}
