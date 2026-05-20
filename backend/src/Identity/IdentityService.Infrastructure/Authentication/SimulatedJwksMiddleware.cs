using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Infrastructure.Authentication;

public class SimulatedJwksMiddleware(RequestDelegate next)
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
