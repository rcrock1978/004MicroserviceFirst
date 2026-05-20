using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using IdentityService.Application.Dtos;

namespace IdentityService.Application.Queries;

public record GetUserProfileByExternalIdQuery(string ExternalId) : IQuery<Result<UserProfileDto>>;

public class GetUserProfileByExternalIdQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetUserProfileByExternalIdQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.ExternalId == request.ExternalId, cancellationToken);

        if (user is null)
        {
            return Result<UserProfileDto>.Failure(Error.NotFoundWithDetails($"User with external ID {request.ExternalId} not found"));
        }

        return Result<UserProfileDto>.Success(MapToDto(user));
    }

    private static UserProfileDto MapToDto(Domain.UserProfile user) => new(
        user.Id,
        user.TenantId.Value,
        user.ExternalId,
        user.Email,
        user.DisplayName,
        user.Roles,
        user.Claims,
        user.IsActive,
        user.CreatedAt,
        user.UpdatedAt
    );
}
