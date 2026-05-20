using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Commands;

public sealed record DeactivateTenantCommand(Guid TenantId) : ICommand<Result<object>>;

public sealed class DeactivateTenantCommandHandler(ITenantDbContext dbContext) : IRequestHandler<DeactivateTenantCommand, Result<object>>
{
    public async Task<Result<object>> Handle(DeactivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        tenant.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
