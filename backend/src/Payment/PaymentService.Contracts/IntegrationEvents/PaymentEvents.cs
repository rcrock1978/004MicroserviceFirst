namespace PaymentService.Contracts;

public sealed record PaymentProcessedEvent(Guid PaymentId, Guid OrderId, Guid TenantId, decimal Amount, string ProviderReference, DateTime OccurredOn);
public sealed record PaymentFailedEvent(Guid PaymentId, Guid OrderId, Guid TenantId, decimal Amount, string Reason, DateTime OccurredOn);
public sealed record PaymentRefundedEvent(Guid PaymentId, Guid OrderId, Guid TenantId, decimal Amount, string? ProviderReference, DateTime OccurredOn);
