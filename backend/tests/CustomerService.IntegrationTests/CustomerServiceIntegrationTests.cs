using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using CustomerService.Application.Commands;

namespace CustomerService.IntegrationTests;

public class CustomerServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly CustomerServiceTestFactory _factory;

    public CustomerServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _factory = new CustomerServiceTestFactory(fixture.PostgresConnectionString, fixture.RabbitMqHost);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreated()
    {
        var client = _factory.CreateClient();
        var command = new CreateCustomerCommand("test@example.com", "Test Customer", "555-1234");
        var response = await client.PostAsJsonAsync("/api/customers", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
