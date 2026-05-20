using System.Security.Claims;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SaaSCommon.Application.Behaviors;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.MassTransit;
using SaaSCommon.Infrastructure.OpenTelemetry;
using SaaSCommon.Infrastructure.Resilience;
using SaaSCommon.Infrastructure.Tenancy;
using IdentityService.Application;
using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Infrastructure.Authentication;
using IdentityService.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<IdentityDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("Identity");
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.MigrationsAssembly("IdentityService.Infrastructure");
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
    });
});
builder.Services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

// Current tenant
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<SyncUserFromIdPCommand>();
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<SyncUserFromIdPCommand>();

// MassTransit with outbox
builder.Services.AddMassTransitWithOutbox<IdentityDbContext>();

// OpenTelemetry
builder.Services.AddOpenTelemetryInstrumentation("IdentityService", builder.Configuration);

// Resilience
builder.Services.AddResiliencePipelines();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>(tags: new[] { "ready" });

// JWT Auth
var jwtAuthority = builder.Configuration["Jwt:Authority"];
if (!string.IsNullOrWhiteSpace(jwtAuthority) && !builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtAuthority;
            options.Audience = builder.Configuration["Jwt:Audience"];
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
    builder.Services.AddAuthorization();
}

var app = builder.Build();

// Simulated JWKS middleware for local dev
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<SimulatedJwksMiddleware>();
}
else
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Health check endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/startup", new HealthCheckOptions
{
    Predicate = _ => false,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// Scalar docs
app.MapScalarApiReference(options =>
{
    options.Title = "Identity Service API";
});

// Minimal API endpoints
app.MapGet("/api/users/me", async (HttpContext httpContext, ISender sender) =>
{
    var externalId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? httpContext.User.FindFirst("sub")?.Value;

    if (string.IsNullOrWhiteSpace(externalId))
    {
        return Results.Unauthorized();
    }

    var result = await sender.Send(new GetUserProfileByExternalIdQuery(externalId));
    return result.Match(
        onSuccess: dto => Results.Ok(dto),
        onFailure: error => error.Code switch
        {
            "Error.NotFound" => Results.NotFound(),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError)
        });
})
.RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

app.MapPost("/api/users/sync", async (SyncUserFromIdPCommand command, ISender sender) =>
{
    var result = await sender.Send(command);
    return result.Match(
        onSuccess: id => Results.Ok(new { Id = id }),
        onFailure: error => error.Code switch
        {
            "Error.Validation" => Results.BadRequest(error),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError)
        });
})
.RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

app.MapGet("/api/users/{id:guid}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetUserProfileByIdQuery(id));
    return result.Match(
        onSuccess: dto => Results.Ok(dto),
        onFailure: error => error.Code switch
        {
            "Error.NotFound" => Results.NotFound(),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError)
        });
})
.RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

app.MapGet("/api/users", async (ISender sender) =>
{
    var result = await sender.Send(new GetUserProfilesByTenantQuery());
    return result.Match(
        onSuccess: dto => Results.Ok(dto),
        onFailure: error => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError));
})
.RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

app.Run();

public partial class Program { }
