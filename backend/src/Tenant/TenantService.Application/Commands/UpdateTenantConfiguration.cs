using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Commands;

public sealed record UpdateTenantConfigurationCommand(Guid TenantId, TenantConfiguration Configuration) : ICommand<Result<object>>;

public sealed class UpdateTenantConfigurationCommandHandler(ITenantDbContext dbContext) : IRequestHandler<UpdateTenantConfigurationCommand, Result<object>>
{
    public async Task<Result<object>> Handle(UpdateTenantConfigurationCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { request.TenantId }, cancellationToken);
        if (tenant is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Tenant '{request.TenantId}' not found."));
        }

        tenant.UpdateConfiguration(request.Configuration);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
