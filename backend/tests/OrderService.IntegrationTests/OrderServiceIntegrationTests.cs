using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using OrderService.Application.Commands;

namespace OrderService.IntegrationTests;

public class OrderServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly OrderServiceTestFactory _factory;

    public OrderServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _factory = new OrderServiceTestFactory(fixture.PostgresConnectionString, fixture.RabbitMqHost);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreated()
    {
        var client = _factory.CreateClient();
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            new List<OrderItemDto> { new(Guid.NewGuid(), 1, 10.00m) },
            Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/orders", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
