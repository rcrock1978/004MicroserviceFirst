using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using IdentityService.Domain;

namespace IdentityService.Application.Commands;

public record SyncUserFromIdPCommand(
    TenantId TenantId,
    string ExternalId,
    string Email,
    string DisplayName,
    List<string> Roles,
    Dictionary<string, string> Claims
) : ICommand<Result<Guid>>;

public class SyncUserFromIdPCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<SyncUserFromIdPCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SyncUserFromIdPCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId == TenantId.Empty)
        {
            return Result<Guid>.Failure(Error.ValidationWithDetails("Tenant ID is required."));
        }

        var existing = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.ExternalId == request.ExternalId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateFromIdP(request.Email, request.DisplayName, request.Roles, request.Claims);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(existing.Id);
        }

        var user = new UserProfile(
            request.TenantId,
            request.ExternalId,
            request.Email,
            request.DisplayName,
            request.Roles,
            request.Claims);

        dbContext.UserProfiles.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(user.Id);
    }
}
