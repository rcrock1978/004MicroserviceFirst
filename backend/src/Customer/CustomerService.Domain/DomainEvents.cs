using SaaSCommon.Domain;

namespace CustomerService.Domain;

public sealed record CustomerCreated(Guid CustomerId, string Email, string Name, TenantId TenantId) : DomainEvent;
public sealed record CustomerProfileUpdated(Guid CustomerId, string Name, TenantId TenantId) : DomainEvent;
