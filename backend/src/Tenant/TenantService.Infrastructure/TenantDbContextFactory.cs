using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TenantService.Infrastructure;

public sealed class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=tenant;Username=postgres;Password=postgres");

        return new TenantDbContext(optionsBuilder.Options, new NullCurrentTenantService(), null);
    }

    private sealed class NullCurrentTenantService : SaaSCommon.Infrastructure.Tenancy.ICurrentTenantService
    {
        public SaaSCommon.Domain.TenantId TenantId => SaaSCommon.Domain.TenantId.Empty;
    }
}
