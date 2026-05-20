using FluentValidation;
using OrderService.Application.Commands;

namespace OrderService.Application.Validators;

public sealed class MarkOrderAsShippedCommandValidator : AbstractValidator<MarkOrderAsShippedCommand>
{
    public MarkOrderAsShippedCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");
    }
}
