using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using CustomerService.Application;

namespace CustomerService.Infrastructure.Consumers;

public sealed class OrderCompletedConsumer(ICustomerDbContext dbContext) : IConsumer<OrderCompleted>
{
    public async Task Consume(ConsumeContext<OrderCompleted> context)
    {
        var message = context.Message;

        var history = await dbContext.CustomerOrderHistory
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrderId == message.OrderId, context.CancellationToken);

        if (history is null)
        {
            return;
        }

        history.Status = "Completed";
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
