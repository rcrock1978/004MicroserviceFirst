using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Tenancy;
using Scalar.AspNetCore;
using InventoryService.API.Endpoints;
using InventoryService.API.HealthChecks;
using InventoryService.Application;
using InventoryService.Infrastructure;
using InventoryService.Infrastructure.BackgroundJobs;
using InventoryService.Infrastructure.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddDbContext<InventoryDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Inventory");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("InventoryService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

builder.Services.AddScoped<IInventoryDbContext>(sp => sp.GetRequiredService<InventoryDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<InventoryService.Application.Commands.ReserveStockCommand>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<InventoryService.Application.Commands.ReserveStockCommand>();

builder.Services.AddMassTransitWithOutbox<InventoryDbContext>(cfg =>
{
    cfg.AddConsumer<OrderPlacedConsumer>();
    cfg.AddConsumer<OrderPaymentFailedConsumer>();
    cfg.AddConsumer<OrderCancelledConsumer>();
});

builder.Services.AddOpenTelemetryInstrumentation("InventoryService", builder.Configuration);

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ReservationExpiryJob");
    q.AddJob<ReservationExpiryJob>(jobKey);
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ReservationExpiryJob-trigger")
        .WithCronSchedule("0 * * ? * *")); // Every minute
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/startup", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapInventoryEndpoints();

app.Run();

public partial class Program { }
