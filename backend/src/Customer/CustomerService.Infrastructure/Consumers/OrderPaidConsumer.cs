using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using CustomerService.Application;

namespace CustomerService.Infrastructure.Consumers;

public sealed class OrderPaidConsumer(ICustomerDbContext dbContext) : IConsumer<OrderPaid>
{
    public async Task Consume(ConsumeContext<OrderPaid> context)
    {
        var message = context.Message;

        var history = await dbContext.CustomerOrderHistory
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrderId == message.OrderId, context.CancellationToken);

        if (history is null)
        {
            return;
        }

        history.Status = "Paid";
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
