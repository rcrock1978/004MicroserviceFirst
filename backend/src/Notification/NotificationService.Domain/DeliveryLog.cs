namespace NotificationService.Domain;

public class DeliveryLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; }
    public string? Error { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
