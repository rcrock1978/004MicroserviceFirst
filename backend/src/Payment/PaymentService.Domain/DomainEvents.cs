using SaaSCommon.Domain;

namespace PaymentService.Domain;

public sealed record PaymentProcessed(Guid PaymentId, Guid OrderId, TenantId TenantId, decimal Amount, string ProviderReference) : DomainEvent;
public sealed record PaymentFailed(Guid PaymentId, Guid OrderId, TenantId TenantId, decimal Amount, string Reason) : DomainEvent;
public sealed record PaymentRefunded(Guid PaymentId, Guid OrderId, TenantId TenantId, decimal Amount, string? ProviderReference) : DomainEvent;
