using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using CustomerService.Application;

namespace CustomerService.Infrastructure.Consumers;

public sealed class OrderShippedConsumer(ICustomerDbContext dbContext) : IConsumer<OrderShipped>
{
    public async Task Consume(ConsumeContext<OrderShipped> context)
    {
        var message = context.Message;

        var history = await dbContext.CustomerOrderHistory
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrderId == message.OrderId, context.CancellationToken);

        if (history is null)
        {
            return;
        }

        history.Status = "Shipped";
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
