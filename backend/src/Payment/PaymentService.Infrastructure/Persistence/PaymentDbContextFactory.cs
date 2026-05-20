using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace PaymentService.Infrastructure;

public sealed class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=payment;Username=postgres;Password=postgres");
        return new PaymentDbContext(optionsBuilder.Options, new NullCurrentTenantService(), null);
    }

    private sealed class NullCurrentTenantService : ICurrentTenantService
    {
        public TenantId TenantId => TenantId.Empty;
    }
}
