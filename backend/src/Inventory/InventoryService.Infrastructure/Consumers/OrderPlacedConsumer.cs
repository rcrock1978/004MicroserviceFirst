using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using InventoryService.Application;
using InventoryService.Application.Commands;
using InventoryService.Domain;

namespace InventoryService.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(IInventoryDbContext dbContext) : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;
        var stockItemIds = new List<Guid>();

        foreach (var item in message.Items)
        {
            var stockItem = await dbContext.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId, context.CancellationToken);

            if (stockItem is null)
            {
                await context.Publish(new Contracts.StockReservationFailedEvent(
                    item.ProductId, message.TenantId, message.OrderId, item.Quantity,
                    $"Stock item for product {item.ProductId} not found.", DateTime.UtcNow));
                return;
            }

            try
            {
                stockItem.Reserve(message.OrderId, item.Quantity, TimeSpan.FromMinutes(10));
                stockItemIds.Add(stockItem.Id);
            }
            catch (InvalidOperationException ex)
            {
                await context.Publish(new Contracts.StockReservationFailedEvent(
                    item.ProductId, message.TenantId, message.OrderId, item.Quantity,
                    ex.Message, DateTime.UtcNow));
                return;
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
