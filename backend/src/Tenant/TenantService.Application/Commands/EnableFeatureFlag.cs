using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Commands;

public sealed record EnableFeatureFlagCommand(Guid TenantId, string Key) : ICommand<Result<object>>;

public sealed class EnableFeatureFlagCommandHandler(ITenantDbContext dbContext) : IRequestHandler<EnableFeatureFlagCommand, Result<object>>
{
    public async Task<Result<object>> Handle(EnableFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        tenant.EnableFeatureFlag(request.Key);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
