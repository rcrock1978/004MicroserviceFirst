using Microsoft.EntityFrameworkCore;
using TenantService.Domain;

namespace TenantService.Application;

public interface ITenantDbContext
{
    DbSet<Tenant> Tenants { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
