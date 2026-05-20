using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Application.Ports;
using NotificationService.Domain;
using NotificationService.Contracts.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

public sealed class OrderShippedConsumer(
    INotificationProvider notificationProvider,
    INotificationDbContext dbContext,
    ILogger<OrderShippedConsumer> logger)
    : IConsumer<OrderShippedEvent>
{
    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var message = context.Message;

        var result = await notificationProvider.SendEmailAsync(
            message.CustomerEmail,
            "Shipping Notification",
            $"Your order #{message.OrderId} has been shipped. Tracking number: {message.TrackingNumber}.");

        var log = new DeliveryLog
        {
            EventType = nameof(OrderShippedEvent),
            Recipient = message.CustomerEmail,
            Channel = "Email",
            Status = result.Success ? DeliveryStatus.Success : DeliveryStatus.Failed,
            Error = result.Error,
            SentAt = DateTime.UtcNow
        };

        dbContext.DeliveryLogs.Add(log);

        logger.LogInformation("Shipping notification processed for OrderId={OrderId}", message.OrderId);
    }
}
