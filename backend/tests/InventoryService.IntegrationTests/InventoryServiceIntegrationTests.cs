using FluentAssertions;
using System.Net;

namespace InventoryService.IntegrationTests;

public class InventoryServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly InventoryServiceTestFactory _factory;

    public InventoryServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _factory = new InventoryServiceTestFactory(fixture.PostgresConnectionString, fixture.RabbitMqHost);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStock_NonExistingProduct_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/inventory/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
