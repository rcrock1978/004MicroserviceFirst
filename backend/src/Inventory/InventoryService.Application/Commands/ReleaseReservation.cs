using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Commands;

public sealed record ReleaseReservationCommand(Guid ReservationId) : ICommand<Result<object>>;

public sealed class ReleaseReservationCommandHandler(IInventoryDbContext dbContext) : IRequestHandler<ReleaseReservationCommand, Result<object>>
{
    public async Task<Result<object>> Handle(ReleaseReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations
            .Include(r => r.StockItem)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Reservation '{request.ReservationId}' not found."));
        }

        try
        {
            if (reservation.StockItem is null)
            {
                return Result<object>.Failure(Error.Conflict with { Details = "Reservation is not associated with a stock item." });
            }
            reservation.StockItem.ReleaseReservation(reservation.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<object>.Failure(Error.Conflict with { Details = ex.Message });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
