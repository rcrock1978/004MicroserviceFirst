using SaaSCommon.Domain;

namespace OrderService.Domain;

public sealed record OrderCreated(Guid OrderId, Guid CustomerId, TenantId TenantId) : DomainEvent;
public sealed record OrderPlaced(Guid OrderId, Guid CustomerId, TenantId TenantId, IReadOnlyCollection<OrderItem> Items, decimal TotalAmount) : DomainEvent;
public sealed record OrderPaymentRequested(Guid OrderId, Guid CustomerId, TenantId TenantId, decimal Amount) : DomainEvent;
public sealed record OrderPaid(Guid OrderId, Guid CustomerId, TenantId TenantId, decimal Amount, string ProviderReference) : DomainEvent;
public sealed record OrderPaymentFailed(Guid OrderId, Guid CustomerId, TenantId TenantId, decimal Amount) : DomainEvent;
public sealed record OrderShipped(Guid OrderId, Guid CustomerId, TenantId TenantId) : DomainEvent;
public sealed record OrderCompleted(Guid OrderId, Guid CustomerId, TenantId TenantId) : DomainEvent;
public sealed record OrderCancelled(Guid OrderId, Guid CustomerId, TenantId TenantId) : DomainEvent;
