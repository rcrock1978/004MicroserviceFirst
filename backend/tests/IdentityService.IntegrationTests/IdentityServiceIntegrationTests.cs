using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using IdentityService.Application.Commands;

namespace IdentityService.IntegrationTests;

public class IdentityServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IdentityServiceTestFactory _factory;

    public IdentityServiceIntegrationTests(IntegrationTestFixture fixture)
    {
        _factory = new IdentityServiceTestFactory(fixture.PostgresConnectionString, fixture.RabbitMqHost);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncUser_ShouldCreateUser()
    {
        var client = _factory.CreateClient();
        var command = new SyncUserFromIdPCommand(
            new SaaSCommon.Domain.TenantId(Guid.NewGuid()),
            "ext-123",
            "user@example.com",
            "Test User",
            new List<string>(),
            new Dictionary<string, string>());

        var response = await client.PostAsJsonAsync("/api/users/sync", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
