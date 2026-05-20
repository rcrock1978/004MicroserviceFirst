using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using IdentityService.Application.Dtos;

namespace IdentityService.Application.Queries;

public record GetUserProfileByIdQuery(Guid UserProfileId) : IQuery<Result<UserProfileDto>>;

public class GetUserProfileByIdQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserProfileId, cancellationToken);

        if (user is null)
        {
            return Result<UserProfileDto>.Failure(Error.NotFoundWithDetails($"User {request.UserProfileId} not found"));
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
