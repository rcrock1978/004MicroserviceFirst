using Microsoft.Extensions.Diagnostics.HealthChecks;
using InventoryService.Infrastructure;

namespace InventoryService.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly InventoryDbContext _dbContext;

    public DatabaseHealthCheck(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = _dbContext.Database.CanConnect();
            return Task.FromResult(canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());
        }
        catch
        {
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}
