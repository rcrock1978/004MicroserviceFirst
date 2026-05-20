using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using CustomerService.Application;
using CustomerService.Domain;

namespace CustomerService.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(ICustomerDbContext dbContext) : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;

        var existing = await dbContext.CustomerOrderHistory
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.OrderId == message.OrderId, context.CancellationToken);

        if (existing is not null)
        {
            return;
        }

        var history = new CustomerOrderHistory
        {
            OrderId = message.OrderId,
            CustomerId = message.CustomerId,
            Status = "Placed",
            OrderDate = message.OrderDate
        };
        history.SetTenantId(message.TenantId);

        dbContext.CustomerOrderHistory.Add(history);
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
