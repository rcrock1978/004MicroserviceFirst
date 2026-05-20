using FluentValidation;
using TenantService.Application.Commands;

namespace TenantService.Application.Validators;

public sealed class EnableFeatureFlagCommandValidator : AbstractValidator<EnableFeatureFlagCommand>
{
    public EnableFeatureFlagCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Feature flag key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.");
    }
}
