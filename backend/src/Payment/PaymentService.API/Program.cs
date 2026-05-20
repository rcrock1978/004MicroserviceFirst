using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Tenancy;
using Scalar.AspNetCore;
using PaymentService.API.Endpoints;
using PaymentService.API.HealthChecks;
using PaymentService.Application;
using PaymentService.Application.Ports;
using PaymentService.Infrastructure;
using PaymentService.Infrastructure.Consumers;
using PaymentService.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddDbContext<PaymentDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Payment");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("PaymentService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

builder.Services.AddScoped<IPaymentDbContext>(sp => sp.GetRequiredService<PaymentDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<PaymentService.Application.Commands.ProcessPaymentCommand>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<PaymentService.Application.Commands.ProcessPaymentCommand>();

builder.Services.AddScoped<IPaymentProvider, SimulatedPaymentProvider>(_ =>
    new SimulatedPaymentProvider(
        failBelowAmount: 0,
        failureRatePercent: null,
        latency: TimeSpan.FromMilliseconds(100)));

builder.Services.AddMassTransitWithOutbox<PaymentDbContext>(cfg =>
{
    cfg.AddConsumer<OrderPaymentRequestedConsumer>();
});

builder.Services.AddOpenTelemetryInstrumentation("PaymentService", builder.Configuration);

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

app.MapPaymentEndpoints();

app.Run();

public partial class Program { }
