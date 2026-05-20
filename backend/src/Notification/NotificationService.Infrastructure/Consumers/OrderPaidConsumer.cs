using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Application.Ports;
using NotificationService.Domain;
using NotificationService.Contracts.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

public sealed class OrderPaidConsumer(
    INotificationProvider notificationProvider,
    INotificationDbContext dbContext,
    ILogger<OrderPaidConsumer> logger)
    : IConsumer<OrderPaidEvent>
{
    public async Task Consume(ConsumeContext<OrderPaidEvent> context)
    {
        var message = context.Message;

        var result = await notificationProvider.SendEmailAsync(
            message.CustomerEmail,
            "Payment Receipt",
            $"Your payment of {message.Amount:C} for order #{message.OrderId} has been received.");

        var log = new DeliveryLog
        {
            EventType = nameof(OrderPaidEvent),
            Recipient = message.CustomerEmail,
            Channel = "Email",
            Status = result.Success ? DeliveryStatus.Success : DeliveryStatus.Failed,
            Error = result.Error,
            SentAt = DateTime.UtcNow
        };

        dbContext.DeliveryLogs.Add(log);

        logger.LogInformation("Payment receipt processed for OrderId={OrderId}", message.OrderId);
    }
}
