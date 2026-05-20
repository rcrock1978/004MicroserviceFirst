using FluentAssertions;
using InventoryService.Application.Commands;
using InventoryService.Domain;
using SaaSCommon.Domain;

namespace InventoryService.Application.Tests;

public class ReserveStockCommandHandlerTests
{
    private static TestInventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestInventoryDbContext(options);
    }

    [Fact]
    public async Task Handle_SufficientStock_ShouldCreateReservation()
    {
        await using var dbContext = CreateDbContext();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(productId, 100, new TenantId(Guid.NewGuid()));
        dbContext.StockItems.Add(stockItem);
        await dbContext.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(dbContext);
        var result = await handler.Handle(new ReserveStockCommand(productId, Guid.NewGuid(), 10, stockItem.TenantId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Reservations.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldReturnConflict()
    {
        await using var dbContext = CreateDbContext();
        var productId = Guid.NewGuid();
        var stockItem = StockItem.Create(productId, 5, new TenantId(Guid.NewGuid()));
        dbContext.StockItems.Add(stockItem);
        await dbContext.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(dbContext);
        var result = await handler.Handle(new ReserveStockCommand(productId, Guid.NewGuid(), 10, stockItem.TenantId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ReserveStockCommandHandler(dbContext);
        var result = await handler.Handle(new ReserveStockCommand(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
