using SaaSCommon.Domain;

namespace CustomerService.Contracts;

public sealed record CustomerCreatedEvent(Guid CustomerId, string Email, string Name, TenantId TenantId, DateTime OccurredOn);
public sealed record CustomerProfileUpdatedEvent(Guid CustomerId, string Name, TenantId TenantId, DateTime OccurredOn);
