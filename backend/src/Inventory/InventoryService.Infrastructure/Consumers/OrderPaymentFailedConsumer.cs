using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Contracts;
using InventoryService.Application;
using InventoryService.Domain;

namespace InventoryService.Infrastructure.Consumers;

public sealed class OrderPaymentFailedConsumer(IInventoryDbContext dbContext) : IConsumer<OrderPaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<OrderPaymentFailedEvent> context)
    {
        var reservations = await dbContext.Reservations
            .Include(r => r.StockItem)
            .Where(r => r.OrderId == context.Message.OrderId && r.Status == ReservationStatus.Active)
            .ToListAsync(context.CancellationToken);

        foreach (var reservation in reservations)
        {
            try
            {
                if (reservation.StockItem is not null)
                {
                    reservation.StockItem.ReleaseReservation(reservation.Id);
                }
            }
            catch (InvalidOperationException)
            {
                // Already released or not active; ignore
            }
        }

        if (reservations.Any())
        {
            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
