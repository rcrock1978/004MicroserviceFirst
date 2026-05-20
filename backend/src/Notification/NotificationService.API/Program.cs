using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.API.HealthChecks;
using NotificationService.Infrastructure;
using NotificationService.Infrastructure.Jobs;
using Quartz;
using SaaSCommon.Infrastructure.OpenTelemetry;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationInfrastructure(builder.Configuration);

builder.Services.AddOpenTelemetryInstrumentation("NotificationService", builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("database", tags: ["ready"])
    .AddCheck("live", () => HealthCheckResult.Healthy(), tags: ["live"]);

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("CleanupDeliveryLogJob");
    q.AddJob<CleanupDeliveryLogJob>(jobKey);
    q.AddTrigger(trigger =>
    {
        trigger.ForJob(jobKey)
               .WithIdentity("CleanupDeliveryLogTrigger")
               .WithCronSchedule("0 0 0 * * ?");
    });
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/startup", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();
