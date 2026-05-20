using FluentAssertions;
using System.Text.Json;

namespace ContractTests;

public class CustomerEventContractTests
{
    [Fact]
    public void CustomerCreatedEvent_ShouldSerializeAndDeserialize()
    {
        var original = new
        {
            CustomerId = Guid.NewGuid(),
            Email = "customer@example.com",
            Name = "Test Customer",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<object>(json);

        deserialized.Should().NotBeNull();
    }
}
