using FluentValidation;
using TenantService.Application.Commands;

namespace TenantService.Application.Validators;

public sealed class UpdateTenantConfigurationCommandValidator : AbstractValidator<UpdateTenantConfigurationCommand>
{
    public UpdateTenantConfigurationCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Configuration)
            .NotNull().WithMessage("Configuration is required.");

        RuleFor(x => x.Configuration.TimeZone)
            .MaximumLength(100).WithMessage("TimeZone must not exceed 100 characters.");

        RuleFor(x => x.Configuration.DefaultLanguage)
            .MaximumLength(10).WithMessage("DefaultLanguage must not exceed 10 characters.");

        RuleFor(x => x.Configuration.MaxUsers)
            .GreaterThan(0).WithMessage("MaxUsers must be greater than 0.");
    }
}
