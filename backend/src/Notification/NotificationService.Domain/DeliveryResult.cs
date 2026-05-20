namespace NotificationService.Domain;

public record DeliveryResult(bool Success, string? Error = null);
