namespace NotificationService.Contracts.IntegrationEvents;

public record OrderShippedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid TenantId,
    string CustomerEmail,
    string TrackingNumber,
    DateTime OccurredOn
);
