using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Tenancy;
using Scalar.AspNetCore;
using CustomerService.API.Endpoints;
using CustomerService.API.HealthChecks;
using CustomerService.Application;
using CustomerService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddDbContext<CustomerDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Customer");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("CustomerService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

builder.Services.AddScoped<ICustomerDbContext>(sp => sp.GetRequiredService<CustomerDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CustomerService.Application.Commands.CreateCustomerCommand>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<CustomerService.Application.Commands.CreateCustomerCommand>();

builder.Services.AddMassTransitWithOutbox<CustomerDbContext>(cfg =>
{
    cfg.AddConsumer<CustomerService.Infrastructure.Consumers.OrderPlacedConsumer>();
    cfg.AddConsumer<CustomerService.Infrastructure.Consumers.OrderPaidConsumer>();
    cfg.AddConsumer<CustomerService.Infrastructure.Consumers.OrderShippedConsumer>();
    cfg.AddConsumer<CustomerService.Infrastructure.Consumers.OrderCompletedConsumer>();
});

builder.Services.AddOpenTelemetryInstrumentation("CustomerService", builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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

app.MapCustomerEndpoints();

app.Run();

public partial class Program { }
