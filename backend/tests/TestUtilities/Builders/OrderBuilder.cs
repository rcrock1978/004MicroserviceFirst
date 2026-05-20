using OrderService.Domain;
using SaaSCommon.Domain;

namespace TestUtilities.Builders;

public class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private TenantId _tenantId = new TenantId(Guid.NewGuid());
    private readonly List<OrderItem> _items = new();

    public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public OrderBuilder AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        _items.Add(new OrderItem(productId, quantity, unitPrice));
        return this;
    }

    public Order Build()
    {
        var order = Order.Create(_customerId, _tenantId);
        foreach (var item in _items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }
        return order;
    }
}
