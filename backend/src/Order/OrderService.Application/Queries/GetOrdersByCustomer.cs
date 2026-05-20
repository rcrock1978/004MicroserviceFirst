using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Application.Queries;

public sealed record GetOrdersByCustomerQuery(Guid CustomerId) : IQuery<Result<List<Order>>>;

public sealed class GetOrdersByCustomerQueryHandler(IOrderDbContext dbContext) : IRequestHandler<GetOrdersByCustomerQuery, Result<List<Order>>>
{
    public async Task<Result<List<Order>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<Order>>.Success(orders);
    }
}
