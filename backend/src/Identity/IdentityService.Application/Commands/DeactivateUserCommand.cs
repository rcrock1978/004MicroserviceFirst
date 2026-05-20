using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;

namespace IdentityService.Application.Commands;

public record DeactivateUserCommand(Guid UserProfileId) : ICommand<Result<object>>;

public class DeactivateUserCommandHandler(IIdentityDbContext dbContext)
    : IRequestHandler<DeactivateUserCommand, Result<object>>
{
    public async Task<Result<object>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == request.UserProfileId, cancellationToken);

        if (user is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"User {request.UserProfileId} not found"));
        }

        user.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
