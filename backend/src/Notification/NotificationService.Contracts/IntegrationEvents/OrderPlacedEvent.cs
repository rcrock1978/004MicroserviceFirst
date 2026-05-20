namespace NotificationService.Contracts.IntegrationEvents;

public record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid TenantId,
    string CustomerEmail,
    decimal TotalAmount,
    DateTime OccurredOn
);
