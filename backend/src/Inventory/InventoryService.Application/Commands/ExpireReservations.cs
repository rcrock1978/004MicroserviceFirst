using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Commands;

public sealed record ExpireReservationsCommand : ICommand<Result<int>>;

public sealed class ExpireReservationsCommandHandler(IInventoryDbContext dbContext) : IRequestHandler<ExpireReservationsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expiredReservations = await dbContext.Reservations
            .Include(r => r.StockItem)
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        foreach (var reservation in expiredReservations)
        {
            reservation.Expire();
            if (reservation.StockItem is not null)
            {
                reservation.StockItem.ReleaseReservation(reservation.Id);
            }
        }

        if (expiredReservations.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(expiredReservations.Count);
    }
}
