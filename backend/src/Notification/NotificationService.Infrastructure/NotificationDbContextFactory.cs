using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure;

public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=notification;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly("NotificationService.Infrastructure"));

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
