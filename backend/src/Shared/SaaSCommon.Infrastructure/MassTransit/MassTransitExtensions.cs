using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SaaSCommon.Infrastructure.MassTransit;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithOutbox<TDbContext>(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? configureBus = null)
        where TDbContext : DbContext
    {
        services.AddMassTransit(cfg =>
        {
            cfg.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("saas", false));

            cfg.UsingRabbitMq((context, rabbitCfg) =>
            {
                var host = context.GetRequiredService<IConfiguration>()["RabbitMq:Host"] ?? "localhost";
                var username = context.GetRequiredService<IConfiguration>()["RabbitMq:Username"] ?? "saas";
                var password = context.GetRequiredService<IConfiguration>()["RabbitMq:Password"] ?? "saas";

                rabbitCfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                rabbitCfg.UseMessageRetry(retry =>
                {
                    retry.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2));
                });

                rabbitCfg.ConfigureEndpoints(context);
            });

            cfg.AddEntityFrameworkOutbox<TDbContext>(o =>
            {
                o.QueryMessageLimit = 100;
                o.UsePostgres();
                o.UseBusOutbox();
            });

            configureBus?.Invoke(cfg);
        });

        return services;
    }
}
