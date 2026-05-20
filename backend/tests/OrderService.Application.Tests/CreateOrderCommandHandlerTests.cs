using FluentAssertions;
using OrderService.Application.Commands;
using OrderService.Domain;
using SaaSCommon.Domain;

namespace OrderService.Application.Tests;

public class CreateOrderCommandHandlerTests
{
    private static TestOrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestOrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestOrderDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderWithItems()
    {
        await using var dbContext = CreateDbContext();
        var handler = new CreateOrderCommandHandler(dbContext);
        var customerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var items = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), 2, 10.00m),
            new(Guid.NewGuid(), 1, 5.00m)
        };

        var result = await handler.Handle(new CreateOrderCommand(customerId, items, tenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Orders.Should().ContainSingle();
    }
}
