using MediatR;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Queries;

public sealed record GetTenantFeaturesQuery(Guid TenantId) : IQuery<Result<List<FeatureFlag>>>;

public sealed class GetTenantFeaturesQueryHandler(ITenantDbContext dbContext) : IRequestHandler<GetTenantFeaturesQuery, Result<List<FeatureFlag>>>
{
    public async Task<Result<List<FeatureFlag>>> Handle(GetTenantFeaturesQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<List<FeatureFlag>>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        return Result<List<FeatureFlag>>.Success(tenant.FeatureFlags.ToList());
    }
}
