using FluentValidation;
using SaaSCommon.Domain;

namespace IdentityService.Application.Validators;

public class SyncUserFromIdPValidator : AbstractValidator<Commands.SyncUserFromIdPCommand>
{
    public SyncUserFromIdPValidator()
    {
        RuleFor(x => x.TenantId).NotEqual(TenantId.Empty).WithMessage("Tenant ID is required.");
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Roles).NotNull();
        RuleFor(x => x.Claims).NotNull();
    }
}
