using FluentValidation;

namespace IdentityService.Application.Validators;

public class DeactivateUserValidator : AbstractValidator<Commands.DeactivateUserCommand>
{
    public DeactivateUserValidator()
    {
        RuleFor(x => x.UserProfileId).NotEmpty();
    }
}
