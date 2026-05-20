using FluentAssertions;
using OrderService.Application.Queries;
using OrderService.Domain;
using SaaSCommon.Domain;

namespace OrderService.Application.Tests;

public class GetOrderByIdQueryHandlerTests
{
    private static TestOrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestOrderDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingOrder_ShouldReturnOrder()
    {
        await using var dbContext = CreateDbContext();
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new GetOrderByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
