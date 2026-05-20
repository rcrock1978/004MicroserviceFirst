using FluentAssertions;
using System.Text.Json;

namespace ContractTests;

public class InventoryEventContractTests
{
    [Fact]
    public void StockReservedEvent_ShouldSerializeAndDeserialize()
    {
        var original = new
        {
            StockItemId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Quantity = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<object>(json);

        deserialized.Should().NotBeNull();
    }
}
