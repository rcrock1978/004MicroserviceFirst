using FluentAssertions;
using InventoryService.Application.Commands;
using InventoryService.Domain;
using SaaSCommon.Domain;

namespace InventoryService.Application.Tests;

public class ReleaseReservationCommandHandlerTests
{
    private static TestInventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestInventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestInventoryDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingReservation_ShouldReleaseStock()
    {
        await using var dbContext = CreateDbContext();
        var stockItem = StockItem.Create(Guid.NewGuid(), 100, new TenantId(Guid.NewGuid()));
        var reservation = stockItem.Reserve(Guid.NewGuid(), 10, TimeSpan.FromMinutes(10));
        dbContext.StockItems.Add(stockItem);
        dbContext.Reservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        var handler = new ReleaseReservationCommandHandler(dbContext);
        var result = await handler.Handle(new ReleaseReservationCommand(reservation.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updatedItem = dbContext.StockItems.First();
        updatedItem.QuantityAvailable.Should().Be(100);
    }

    [Fact]
    public async Task Handle_NonExistingReservation_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ReleaseReservationCommandHandler(dbContext);

        var result = await handler.Handle(new ReleaseReservationCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
