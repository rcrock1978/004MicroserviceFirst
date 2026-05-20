using FluentAssertions;
using OrderService.Application.Commands;
using OrderService.Domain;
using SaaSCommon.Domain;

namespace OrderService.Application.Tests;

public class PlaceOrderCommandHandlerTests
{
    private static TestOrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestOrderDbContext(options);
    }

    [Fact]
    public async Task Handle_DraftOrderWithItems_ShouldPlaceOrder()
    {
        await using var dbContext = CreateDbContext();
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var handler = new PlaceOrderCommandHandler(dbContext);
        var result = await handler.Handle(new PlaceOrderCommand(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await dbContext.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new PlaceOrderCommandHandler(dbContext);

        var result = await handler.Handle(new PlaceOrderCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_AlreadyPlacedOrder_ShouldReturnConflict()
    {
        await using var dbContext = CreateDbContext();
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var handler = new PlaceOrderCommandHandler(dbContext);
        var result = await handler.Handle(new PlaceOrderCommand(order.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }
}
