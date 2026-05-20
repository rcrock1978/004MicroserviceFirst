using FluentValidation;
using TenantService.Application.Commands;

namespace TenantService.Application.Validators;

public sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");
    }
}
