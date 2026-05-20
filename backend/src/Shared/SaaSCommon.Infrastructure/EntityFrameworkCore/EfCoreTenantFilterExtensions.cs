using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;

namespace SaaSCommon.Infrastructure.EntityFrameworkCore;

public static class EfCoreTenantFilterExtensions
{
    public static void AddEfCoreTenantFilter<TEntity>(
        this ModelBuilder modelBuilder,
        TenantId currentTenantId)
        where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == currentTenantId);
    }
}
