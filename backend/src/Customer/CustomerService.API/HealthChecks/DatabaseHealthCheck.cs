using Microsoft.Extensions.Diagnostics.HealthChecks;
using CustomerService.Infrastructure;

namespace CustomerService.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly CustomerDbContext _dbContext;

    public DatabaseHealthCheck(CustomerDbContext dbContext)
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
