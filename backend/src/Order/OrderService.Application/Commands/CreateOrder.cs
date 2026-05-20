using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using OrderService.Domain;

namespace OrderService.Application.Commands;

public sealed record CreateOrderCommand(Guid CustomerId, List<OrderItemDto> Items, Guid TenantId) : ICommand<Result<Guid>>;
public sealed record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);

public sealed class CreateOrderCommandHandler(IOrderDbContext dbContext) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.CustomerId, new TenantId(request.TenantId));

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}
