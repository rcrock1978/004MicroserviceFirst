using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Tenancy;
using Scalar.AspNetCore;
using TenantService.API.Endpoints;
using TenantService.API.HealthChecks;
using TenantService.Application;
using TenantService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddDbContext<TenantDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Tenant");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("TenantService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

builder.Services.AddScoped<ITenantDbContext>(sp => sp.GetRequiredService<TenantDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<TenantService.Application.Commands.ProvisionTenantCommand>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<TenantService.Application.Commands.ProvisionTenantCommand>();

builder.Services.AddMassTransitWithOutbox<TenantDbContext>();

builder.Services.AddOpenTelemetryInstrumentation("TenantService", builder.Configuration);

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

app.MapTenantEndpoints();

app.Run();
