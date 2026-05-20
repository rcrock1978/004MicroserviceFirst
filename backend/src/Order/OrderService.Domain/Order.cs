using SaaSCommon.Domain;

namespace OrderService.Domain;

public enum OrderStatus
{
    Draft,
    Placed,
    PaymentPending,
    Paid,
    Shipped,
    Completed,
    Cancelled,
    PaymentFailed
}

public sealed record OrderItem(Guid ProductId, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

public sealed class Order : Entity
{
    public Guid CustomerId { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public decimal TotalAmount => _items.Sum(i => i.LineTotal);
    public string? PaymentProviderReference { get; private set; }

    private Order() { }

    public static Order Create(Guid customerId, TenantId tenantId)
    {
        var order = new Order
        {
            CustomerId = customerId,
            TenantId = tenantId,
            Status = OrderStatus.Draft
        };

        order.AddDomainEvent(new OrderCreated(order.Id, order.CustomerId, order.TenantId));
        return order;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException("Cannot add items to an order that is not in draft status.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        _items.Add(new OrderItem(productId, quantity, unitPrice));
        SetUpdatedAt(DateTime.UtcNow);
    }

    public void Place()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException("Only draft orders can be placed.");
        }

        if (!_items.Any())
        {
            throw new InvalidOperationException("Cannot place an order with no items.");
        }

        Status = OrderStatus.Placed;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderPlaced(Id, CustomerId, TenantId, Items.ToList(), TotalAmount));
    }

    public void MarkPaymentRequested()
    {
        if (Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException("Can only request payment for placed orders.");
        }

        Status = OrderStatus.PaymentPending;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderPaymentRequested(Id, CustomerId, TenantId, TotalAmount));
    }

    public void MarkAsPaid(string providerReference)
    {
        if (Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOperationException("Can only mark payment pending orders as paid.");
        }

        Status = OrderStatus.Paid;
        PaymentProviderReference = providerReference;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderPaid(Id, CustomerId, TenantId, TotalAmount, providerReference));
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException("Can only ship paid orders.");
        }

        Status = OrderStatus.Shipped;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderShipped(Id, CustomerId, TenantId));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Shipped)
        {
            throw new InvalidOperationException("Can only complete shipped orders.");
        }

        Status = OrderStatus.Completed;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderCompleted(Id, CustomerId, TenantId));
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Paid or OrderStatus.Shipped or OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel an order that has already been paid, shipped, or completed.");
        }

        Status = OrderStatus.Cancelled;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderCancelled(Id, CustomerId, TenantId));
    }

    public void MarkPaymentFailed()
    {
        if (Status != OrderStatus.PaymentPending)
        {
            throw new InvalidOperationException("Can only mark payment pending orders as failed.");
        }

        Status = OrderStatus.PaymentFailed;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new OrderPaymentFailed(Id, CustomerId, TenantId, TotalAmount));
    }
}
