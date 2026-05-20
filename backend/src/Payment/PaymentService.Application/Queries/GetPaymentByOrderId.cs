using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using PaymentService.Domain;

namespace PaymentService.Application.Queries;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<Result<Payment>>;

public sealed class GetPaymentByOrderIdQueryHandler(IPaymentDbContext dbContext) : IRequestHandler<GetPaymentByOrderIdQuery, Result<Payment>>
{
    public async Task<Result<Payment>> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        if (payment is null)
        {
            return Result<Payment>.Failure(Error.NotFoundWithDetails($"Payment for order '{request.OrderId}' not found."));
        }

        return Result<Payment>.Success(payment);
    }
}
