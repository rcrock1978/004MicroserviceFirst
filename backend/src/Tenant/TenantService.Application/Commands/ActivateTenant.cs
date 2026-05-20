using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Commands;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand<Result<object>>;

public sealed class ActivateTenantCommandHandler(ITenantDbContext dbContext) : IRequestHandler<ActivateTenantCommand, Result<object>>
{
    public async Task<Result<object>> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        tenant.Activate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
