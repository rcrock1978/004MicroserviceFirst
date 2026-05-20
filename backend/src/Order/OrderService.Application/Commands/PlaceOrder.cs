using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Application.Commands;

public sealed record PlaceOrderCommand(Guid OrderId) : ICommand<Result<object>>;

public sealed class PlaceOrderCommandHandler(IOrderDbContext dbContext) : IRequestHandler<PlaceOrderCommand, Result<object>>
{
    public async Task<Result<object>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync(new object[] { request.OrderId }, cancellationToken);
        if (order is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Order '{request.OrderId}' not found."));
        }

        try
        {
            order.Place();
        }
        catch (InvalidOperationException ex)
        {
            return Result<object>.Failure(Error.Conflict with { Details = ex.Message });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
