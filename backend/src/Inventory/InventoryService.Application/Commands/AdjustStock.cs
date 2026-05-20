using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using InventoryService.Domain;

namespace InventoryService.Application.Commands;

public sealed record AdjustStockCommand(Guid ProductId, int Delta, Guid TenantId) : ICommand<Result<object>>;

public sealed class AdjustStockCommandHandler(IInventoryDbContext dbContext) : IRequestHandler<AdjustStockCommand, Result<object>>
{
    public async Task<Result<object>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await dbContext.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stockItem is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Stock item for product '{request.ProductId}' not found."));
        }

        try
        {
            stockItem.AdjustStock(request.Delta);
        }
        catch (InvalidOperationException ex)
        {
            return Result<object>.Failure(Error.Conflict with { Details = ex.Message });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(new object());
    }
}
