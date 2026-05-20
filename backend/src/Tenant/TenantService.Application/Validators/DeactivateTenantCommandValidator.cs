using FluentValidation;
using TenantService.Application.Commands;

namespace TenantService.Application.Validators;

public sealed class DeactivateTenantCommandValidator : AbstractValidator<DeactivateTenantCommand>
{
    public DeactivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");
    }
}
