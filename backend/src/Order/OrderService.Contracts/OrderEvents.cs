using SaaSCommon.Domain;

namespace OrderService.Contracts;

public sealed record OrderPlaced(Guid OrderId, Guid CustomerId, TenantId TenantId, DateTime OrderDate, DateTime OccurredOn);
public sealed record OrderPaid(Guid OrderId, TenantId TenantId, DateTime OccurredOn);
public sealed record OrderShipped(Guid OrderId, TenantId TenantId, DateTime OccurredOn);
public sealed record OrderCompleted(Guid OrderId, TenantId TenantId, DateTime OccurredOn);
