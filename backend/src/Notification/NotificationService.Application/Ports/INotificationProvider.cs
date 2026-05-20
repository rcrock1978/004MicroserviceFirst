using NotificationService.Domain;

namespace NotificationService.Application.Ports;

public interface INotificationProvider
{
    Task<DeliveryResult> SendEmailAsync(string to, string subject, string body);
    Task<DeliveryResult> SendSmsAsync(string to, string message);
    Task<DeliveryResult> SendPushAsync(string deviceToken, string title, string body);
}
