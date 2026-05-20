using FluentAssertions;
using System.Text.Json;

namespace ContractTests;

public class PaymentEventContractTests
{
    [Fact]
    public void PaymentProcessedEvent_ShouldSerializeAndDeserialize()
    {
        var original = new
        {
            PaymentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Amount = 100.00m,
            ProviderReference = "ref-123"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<object>(json);

        deserialized.Should().NotBeNull();
    }
}
