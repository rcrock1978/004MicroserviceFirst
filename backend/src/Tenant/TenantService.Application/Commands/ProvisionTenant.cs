using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Application.Commands;

public sealed record ProvisionTenantCommand(string Name, string Slug, TenantConfiguration? Configuration = null) : ICommand<Result<Guid>>;

public sealed class ProvisionTenantCommandHandler(ITenantDbContext dbContext) : IRequestHandler<ProvisionTenantCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ProvisionTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Slug == request.Slug, cancellationToken);
        if (existing is not null)
        {
            return Result<Guid>.Failure(Error.Conflict with { Details = $"Tenant with slug '{request.Slug}' already exists." });
        }

        var tenant = Tenant.Provision(request.Name, request.Slug, request.Configuration);
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id);
    }
}
