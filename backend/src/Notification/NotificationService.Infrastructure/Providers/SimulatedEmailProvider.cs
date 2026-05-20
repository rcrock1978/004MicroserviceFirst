using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Domain;

namespace NotificationService.Infrastructure.Providers;

public sealed class SimulatedEmailProvider(ILogger<SimulatedEmailProvider> logger) : INotificationProvider
{
    public Task<DeliveryResult> SendEmailAsync(string to, string subject, string body)
    {
        logger.LogInformation(
            "[SimulatedEmail] To={To}, Subject={Subject}, Body={Body}",
            to, subject, body);

        return Task.FromResult(new DeliveryResult(true));
    }

    public Task<DeliveryResult> SendSmsAsync(string to, string message)
    {
        logger.LogInformation(
            "[SimulatedSMS] To={To}, Message={Message}",
            to, message);

        return Task.FromResult(new DeliveryResult(true));
    }

    public Task<DeliveryResult> SendPushAsync(string deviceToken, string title, string body)
    {
        logger.LogInformation(
            "[SimulatedPush] DeviceToken={DeviceToken}, Title={Title}, Body={Body}",
            deviceToken, title, body);

        return Task.FromResult(new DeliveryResult(true));
    }
}
