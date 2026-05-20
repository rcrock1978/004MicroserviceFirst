using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using PaymentService.Application.Ports;
using PaymentService.Domain;

namespace PaymentService.Application.Commands;

public sealed record ProcessPaymentCommand(Guid OrderId, decimal Amount, Guid TenantId) : ICommand<Result<Guid>>;

public sealed class ProcessPaymentCommandHandler(IPaymentDbContext dbContext, IPaymentProvider paymentProvider) : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = Payment.Create(request.OrderId, request.Amount, new TenantId(request.TenantId));
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = await paymentProvider.ProcessPaymentAsync(
            new PaymentRequest(request.OrderId, request.Amount, request.TenantId),
            cancellationToken);

        if (result.Success && result.ProviderReference is not null)
        {
            payment.Process(result.ProviderReference);
        }
        else
        {
            payment.Fail(result.ErrorMessage ?? "Payment processing failed.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(payment.Id);
    }
}
