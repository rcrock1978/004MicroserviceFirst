using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Commands;

public sealed record ReserveStockCommand(Guid ProductId, Guid OrderId, int Quantity, Guid TenantId, TimeSpan? Ttl = null) : ICommand<Result<Guid>>;

public sealed class ReserveStockCommandHandler(IInventoryDbContext dbContext) : IRequestHandler<ReserveStockCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await dbContext.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
        {
            return Result<Guid>.Failure(Error.NotFoundWithDetails($"Stock item for product '{request.ProductId}' not found."));
        }

        var ttl = request.Ttl ?? TimeSpan.FromMinutes(10);

        try
        {
            var reservation = stockItem.Reserve(request.OrderId, request.Quantity, ttl);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(reservation.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(Error.Conflict with { Details = ex.Message });
        }
    }
}
