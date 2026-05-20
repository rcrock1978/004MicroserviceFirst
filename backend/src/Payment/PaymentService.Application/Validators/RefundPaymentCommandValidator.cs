using FluentValidation;
using PaymentService.Application.Commands;

namespace PaymentService.Application.Validators;

public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
