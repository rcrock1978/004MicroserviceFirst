using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Tenancy;
using Scalar.AspNetCore;
using OrderService.API.Endpoints;
using OrderService.API.HealthChecks;
using OrderService.Application;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Sagas;
using InventoryService.Contracts;
using PaymentService.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddDbContext<OrderDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Order");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("OrderService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});

builder.Services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<OrderService.Application.Commands.CreateOrderCommand>();
});

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

builder.Services.AddValidatorsFromAssemblyContaining<OrderService.Application.Commands.CreateOrderCommand>();

builder.Services.AddMassTransitWithOutbox<OrderDbContext>(cfg =>
{
    cfg.AddSagaStateMachine<OrderPlacementStateMachine, OrderPlacementSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.AddDbContext<DbContext, OrderDbContext>((provider, options) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Order");
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly("OrderService.Infrastructure");
                });
            });
        });
});

builder.Services.AddOpenTelemetryInstrumentation("OrderService", builder.Configuration);

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

app.MapOrderEndpoints();

app.Run();

public partial class Program { }
