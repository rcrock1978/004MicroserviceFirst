using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderService.Infrastructure;

namespace OrderService.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly OrderDbContext _dbContext;

    public DatabaseHealthCheck(OrderDbContext dbContext)
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
