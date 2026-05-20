namespace OrderService.Contracts;

public sealed record OrderCreatedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, DateTime OccurredOn);
public sealed record OrderPlacedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, List<OrderItemDto> Items, decimal TotalAmount, DateTime OccurredOn);
public sealed record OrderPaymentRequestedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, decimal Amount, DateTime OccurredOn);
public sealed record OrderPaidEvent(Guid OrderId, Guid CustomerId, Guid TenantId, decimal Amount, string ProviderReference, DateTime OccurredOn);
public sealed record OrderPaymentFailedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, decimal Amount, DateTime OccurredOn);
public sealed record OrderShippedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, DateTime OccurredOn);
public sealed record OrderCompletedEvent(Guid OrderId, Guid CustomerId, Guid TenantId, DateTime OccurredOn);
public sealed record OrderCancelledEvent(Guid OrderId, Guid CustomerId, Guid TenantId, DateTime OccurredOn);

public sealed record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);
