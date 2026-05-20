using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using PaymentService.Application.Ports;
using PaymentService.Domain;

namespace PaymentService.Application.Commands;

public sealed record RefundPaymentCommand(Guid PaymentId) : ICommand<Result<object>>;

public sealed class RefundPaymentCommandHandler(IPaymentDbContext dbContext, IPaymentProvider paymentProvider) : IRequestHandler<RefundPaymentCommand, Result<object>>
{
    public async Task<Result<object>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments.FindAsync(new object[] { request.PaymentId }, cancellationToken);
        if (payment is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Payment '{request.PaymentId}' not found."));
        }

        if (payment.ProviderReference is null)
        {
            return Result<object>.Failure(Error.Validation with { Details = "Payment has no provider reference and cannot be refunded." });
        }

        var result = await paymentProvider.RefundPaymentAsync(payment.ProviderReference, cancellationToken);
        if (!result.Success)
        {
            return Result<object>.Failure(Error.Conflict with { Details = $"Refund failed: {result.ErrorMessage}" });
        }

        try
        {
            payment.Refund();
        }
        catch (InvalidOperationException ex)
        {
            return Result<object>.Failure(Error.Conflict with { Details = ex.Message });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
