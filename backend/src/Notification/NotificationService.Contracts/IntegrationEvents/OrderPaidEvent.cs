namespace NotificationService.Contracts.IntegrationEvents;

public record OrderPaidEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid TenantId,
    string CustomerEmail,
    decimal Amount,
    DateTime OccurredOn
);
