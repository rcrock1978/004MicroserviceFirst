using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Application.Queries;

public sealed record GetOrdersByStatusQuery(OrderStatus Status) : IQuery<Result<List<Order>>>;

public sealed class GetOrdersByStatusQueryHandler(IOrderDbContext dbContext) : IRequestHandler<GetOrdersByStatusQuery, Result<List<Order>>>
{
    public async Task<Result<List<Order>>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Where(o => o.Status == request.Status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<Order>>.Success(orders);
    }
}
