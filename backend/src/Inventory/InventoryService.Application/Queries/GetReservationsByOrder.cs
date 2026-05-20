using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Queries;

public sealed record GetReservationsByOrderQuery(Guid OrderId) : IQuery<Result<List<Reservation>>>;

public sealed class GetReservationsByOrderQueryHandler(IInventoryDbContext dbContext) : IRequestHandler<GetReservationsByOrderQuery, Result<List<Reservation>>>
{
    public async Task<Result<List<Reservation>>> Handle(GetReservationsByOrderQuery request, CancellationToken cancellationToken)
    {
        var reservations = await dbContext.Reservations
            .Where(r => r.OrderId == request.OrderId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<List<Reservation>>.Success(reservations);
    }
}
