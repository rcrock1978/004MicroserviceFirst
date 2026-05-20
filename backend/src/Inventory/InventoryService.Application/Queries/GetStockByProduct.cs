using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Queries;

public sealed record GetStockByProductQuery(Guid ProductId) : IQuery<Result<StockItem>>;

public sealed class GetStockByProductQueryHandler(IInventoryDbContext dbContext) : IRequestHandler<GetStockByProductQuery, Result<StockItem>>
{
    public async Task<Result<StockItem>> Handle(GetStockByProductQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await dbContext.StockItems
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
        {
            return Result<StockItem>.Failure(Error.NotFoundWithDetails($"Stock item for product '{request.ProductId}' not found."));
        }

        return Result<StockItem>.Success(stockItem);
    }
}
