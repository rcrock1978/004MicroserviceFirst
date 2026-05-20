using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Infrastructure;

namespace NotificationService.API.HealthChecks;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly NotificationDbContext _dbContext;

    public DatabaseHealthCheck(NotificationDbContext dbContext)
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
