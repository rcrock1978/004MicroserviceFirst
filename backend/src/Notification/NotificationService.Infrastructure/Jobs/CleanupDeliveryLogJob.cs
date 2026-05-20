using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Domain;
using Quartz;

namespace NotificationService.Infrastructure.Jobs;

public sealed class CleanupDeliveryLogJob(
    NotificationDbContext dbContext,
    ILogger<CleanupDeliveryLogJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var oldLogs = dbContext.DeliveryLogs.Where(l => l.SentAt < cutoff);
        dbContext.DeliveryLogs.RemoveRange(oldLogs);
        var count = await dbContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Cleaned up {Count} delivery logs older than 30 days", count);
    }
}
