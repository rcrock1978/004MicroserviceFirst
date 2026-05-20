using MediatR;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Queries;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<Result<Tenant>>;

public sealed class GetTenantByIdQueryHandler(ITenantDbContext dbContext) : IRequestHandler<GetTenantByIdQuery, Result<Tenant>>
{
    public async Task<Result<Tenant>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<Tenant>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        return Result<Tenant>.Success(tenant);
    }
}
