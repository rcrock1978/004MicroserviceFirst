using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;

namespace IdentityService.Application.Commands;

public record UpdateUserClaimsCommand(
    Guid UserProfileId,
    Dictionary<string, string> Claims
) : ICommand<Result<object>>;

public class UpdateUserClaimsCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<UpdateUserClaimsCommand, Result<object>>
{
    public async Task<Result<object>> Handle(UpdateUserClaimsCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == request.UserProfileId, cancellationToken);

        if (user is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"User {request.UserProfileId} not found"));
        }

        user.UpdateClaims(request.Claims);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
