using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SaaSCommon.Infrastructure.OpenTelemetry;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryInstrumentation(
        this IServiceCollection services,
        string serviceName,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Otlp:Endpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MassTransit")
                    .AddEntityFrameworkCoreInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                }
            });

        return services;
    }
}
