using FluentAssertions;
using System.Net;

namespace PaymentService.IntegrationTests;

public class PaymentServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly PaymentServiceTestFactory _factory;

    public PaymentServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _factory = new PaymentServiceTestFactory(fixture.PostgresConnectionString, fixture.RabbitMqHost);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaymentByOrderId_NonExisting_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/payments/order/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
