using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Application.Ports;
using NotificationService.Domain;
using NotificationService.Contracts.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(
    INotificationProvider notificationProvider,
    INotificationDbContext dbContext,
    ILogger<OrderPlacedConsumer> logger)
    : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var message = context.Message;

        var result = await notificationProvider.SendEmailAsync(
            message.CustomerEmail,
            "Order Confirmation",
            $"Your order #{message.OrderId} has been placed. Total: {message.TotalAmount:C}.");

        var log = new DeliveryLog
        {
            EventType = nameof(OrderPlacedEvent),
            Recipient = message.CustomerEmail,
            Channel = "Email",
            Status = result.Success ? DeliveryStatus.Success : DeliveryStatus.Failed,
            Error = result.Error,
            SentAt = DateTime.UtcNow
        };

        dbContext.DeliveryLogs.Add(log);

        logger.LogInformation("Order confirmation processed for OrderId={OrderId}", message.OrderId);
    }
}
