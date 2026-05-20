using Microsoft.AspNetCore.Http;
using SaaSCommon.Domain;

namespace SaaSCommon.Infrastructure.Tenancy;

public interface ICurrentTenantService
{
    TenantId TenantId { get; }
}

public sealed class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantId TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                return TenantId.Empty;
            }

            var headerValue = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(headerValue) || !Guid.TryParse(headerValue, out var tenantGuid))
            {
                return TenantId.Empty;
            }

            return new TenantId(tenantGuid);
        }
    }
}
