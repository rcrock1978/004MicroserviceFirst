using MediatR;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Application.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<Result<Order>>;

public sealed class GetOrderByIdQueryHandler(IOrderDbContext dbContext) : IRequestHandler<GetOrderByIdQuery, Result<Order>>
{
    public async Task<Result<Order>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
        {
            return Result<Order>.Failure(Error.NotFoundWithDetails($"Order '{request.OrderId}' not found."));
        }

        return Result<Order>.Success(order);
    }
}
