using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("External", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("tenant", context =>
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();
        if (string.IsNullOrEmpty(tenantId))
            tenantId = "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            tenantId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("ip", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// Health checks
builder.Services.AddHealthChecks();

// HttpClient for health aggregation
builder.Services.AddHttpClient();

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Simulated JWT for local dev
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<SimulatedJwtMiddleware>();
}
else
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Claims forwarding
app.UseMiddleware<ClaimsForwardingMiddleware>();

// Rate limiting
app.UseRateLimiter();

// CORS
app.UseCors("External");

// Standard health endpoints
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
    Predicate = _ => false,
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

// Aggregate health
app.MapGet("/health/aggregate", async (IHttpClientFactory httpClientFactory, IConfiguration config, CancellationToken cancellationToken) =>
{
    var clusters = new Dictionary<string, string>
    {
        ["identity"] = "identity",
        ["tenant"] = "tenant",
        ["order"] = "order",
        ["customer"] = "customer"
    };

    var statuses = new Dictionary<string, object>();
    bool allHealthy = true;

    foreach (var (name, clusterId) in clusters)
    {
        var address = config[$"ReverseProxy:Clusters:{clusterId}:Destinations:{clusterId}:Address"];
        if (string.IsNullOrWhiteSpace(address))
        {
            statuses[name] = new { status = "Unknown", error = "Address not configured" };
            allHealthy = false;
            continue;
        }

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync($"{address}/health/ready", cancellationToken);
            var healthy = response.StatusCode == HttpStatusCode.OK;
            statuses[name] = new { status = healthy ? "Healthy" : "Unhealthy", httpStatus = (int)response.StatusCode };
            if (!healthy) allHealthy = false;
        }
        catch (Exception ex)
        {
            statuses[name] = new { status = "Unhealthy", error = ex.Message };
            allHealthy = false;
        }
    }

    var result = new { overall = allHealthy ? "Healthy" : "Unhealthy", services = statuses };
    return allHealthy
        ? Results.Ok(result)
        : Results.Json(result, statusCode: StatusCodes.Status503ServiceUnavailable);
});

// Scalar docs
app.MapScalarApiReference(options =>
{
    options.Title = "Gateway API";
});

// YARP
app.MapReverseProxy()
   .RequireRateLimiting("tenant")
   .RequireRateLimiting("ip");

app.Run();

public class SimulatedJwtMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var token = authHeader["Bearer ".Length..];
            var principal = ParseToken(token);
            if (principal is not null)
            {
                context.User = principal;
            }
        }

        await next(context);
    }

    private static ClaimsPrincipal? ParseToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payloadJson = Base64UrlDecode(parts[1]);
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
            if (payload is null)
                return null;

            var claims = new List<Claim>();
            foreach (var kvp in payload)
            {
                if (kvp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in kvp.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(kvp.Key, element.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                }
            }

            var identity = new ClaimsIdentity(claims, "SimulatedJwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    private static string Base64UrlDecode(string input)
    {
        var pad = input.Length % 4;
        var padded = pad switch
        {
            2 => input + "==",
            3 => input + "=",
            _ => input
        };
        var base64 = padded.Replace('-', '+').Replace('_', '/');
        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}

public class ClaimsForwardingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst("sub")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-User-Id"] = userId;
            }

            var tenantId = user.FindFirst("tenant_id")?.Value
                ?? user.FindFirst("tid")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                context.Request.Headers["X-Tenant-Id"] = tenantId;
            }

            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => c.Value)
                .ToList();
            if (roles.Count > 0)
            {
                context.Request.Headers["X-Roles"] = string.Join(",", roles);
            }
        }

        await next(context);
    }
}
