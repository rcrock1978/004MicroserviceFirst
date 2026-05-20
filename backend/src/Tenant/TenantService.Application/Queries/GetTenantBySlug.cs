using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Queries;

public sealed record GetTenantBySlugQuery(string Slug) : IQuery<Result<Tenant>>;

public sealed class GetTenantBySlugQueryHandler(ITenantDbContext dbContext) : IRequestHandler<GetTenantBySlugQuery, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(GetTenantBySlugQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Slug == request.Slug, cancellationToken);
        if (tenant is null)
        {
            return Result<Tenant>.Failure(Error.NotFoundWithDetails($"Tenant with slug '{request.Slug}' not found."));
        }

        return Result<Tenant>.Success(tenant);
    }
}
