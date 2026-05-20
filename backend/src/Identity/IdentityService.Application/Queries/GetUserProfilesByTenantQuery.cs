using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using IdentityService.Application.Dtos;

namespace IdentityService.Application.Queries;

public record GetUserProfilesByTenantQuery : IQuery<Result<List<UserProfileDto>>>;

public class GetUserProfilesByTenantQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetUserProfilesByTenantQuery, Result<List<UserProfileDto>>>
{
    public async Task<Result<List<UserProfileDto>>> Handle(GetUserProfilesByTenantQuery request, CancellationToken cancellationToken)
    {
        var users = await dbContext.UserProfiles
            .AsNoTracking()
            .Select(u => new UserProfileDto(
                u.Id,
                u.TenantId.Value,
                u.ExternalId,
                u.Email,
                u.DisplayName,
                u.Roles,
                u.Claims,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<UserProfileDto>>.Success(users);
    }
}
