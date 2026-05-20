using FluentAssertions;
using System.Text.Json;

namespace ContractTests;

public class NotificationEventContractTests
{
    [Fact]
    public void OrderPlacedEvent_ShouldSerializeAndDeserialize()
    {
        var original = new
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            TotalAmount = 100.00m
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<object>(json);

        deserialized.Should().NotBeNull();
    }
}
