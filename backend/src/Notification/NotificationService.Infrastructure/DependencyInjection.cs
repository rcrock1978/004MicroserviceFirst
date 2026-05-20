using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application;
using NotificationService.Application.Ports;
using NotificationService.Domain;
using NotificationService.Infrastructure.Consumers;
using NotificationService.Infrastructure.Jobs;
using NotificationService.Infrastructure.Providers;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("Notification");
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("NotificationService.Infrastructure");
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            });
        });

        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddSingleton<INotificationProvider, SimulatedEmailProvider>();

        services.AddMassTransit(cfg =>
        {
            cfg.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("saas", false));

            cfg.AddConsumer<UserProfileCreatedConsumer>();
            cfg.AddConsumer<OrderPlacedConsumer>();
            cfg.AddConsumer<OrderPaidConsumer>();
            cfg.AddConsumer<OrderShippedConsumer>();

            cfg.AddEntityFrameworkOutbox<NotificationDbContext>(o =>
            {
                o.QueryMessageLimit = 100;
                o.UsePostgres();
                o.UseBusOutbox();
            });

            cfg.UsingRabbitMq((context, rabbitCfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var username = configuration["RabbitMq:Username"] ?? "saas";
                var password = configuration["RabbitMq:Password"] ?? "saas";

                rabbitCfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                rabbitCfg.UseMessageRetry(retry =>
                {
                    retry.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2));
                });

                rabbitCfg.ReceiveEndpoint("notification-user-profile-created", e =>
                {
                    e.ConfigureConsumer<UserProfileCreatedConsumer>(context);
                    e.UseEntityFrameworkOutbox<NotificationDbContext>(context);
                });

                rabbitCfg.ReceiveEndpoint("notification-order-placed", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(context);
                    e.UseEntityFrameworkOutbox<NotificationDbContext>(context);
                });

                rabbitCfg.ReceiveEndpoint("notification-order-paid", e =>
                {
                    e.ConfigureConsumer<OrderPaidConsumer>(context);
                    e.UseEntityFrameworkOutbox<NotificationDbContext>(context);
                });

                rabbitCfg.ReceiveEndpoint("notification-order-shipped", e =>
                {
                    e.ConfigureConsumer<OrderShippedConsumer>(context);
                    e.UseEntityFrameworkOutbox<NotificationDbContext>(context);
                });
            });
        });

        return services;
    }
}
