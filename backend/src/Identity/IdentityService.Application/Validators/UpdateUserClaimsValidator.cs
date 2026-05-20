using FluentValidation;

namespace IdentityService.Application.Validators;

public class UpdateUserClaimsValidator : AbstractValidator<Commands.UpdateUserClaimsCommand>
{
    public UpdateUserClaimsValidator()
    {
        RuleFor(x => x.UserProfileId).NotEmpty();
        RuleFor(x => x.Claims).NotNull();
    }
}
