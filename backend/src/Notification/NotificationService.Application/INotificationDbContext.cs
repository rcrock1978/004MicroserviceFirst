using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Application;

public interface INotificationDbContext
{
    DbSet<DeliveryLog> DeliveryLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
