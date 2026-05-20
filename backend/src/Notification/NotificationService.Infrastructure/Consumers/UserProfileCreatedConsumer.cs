using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application;
using NotificationService.Application.Ports;
using NotificationService.Domain;
using IdentityService.Contracts.IntegrationEvents;

namespace NotificationService.Infrastructure.Consumers;

public sealed class UserProfileCreatedConsumer(
    INotificationProvider notificationProvider,
    INotificationDbContext dbContext,
    ILogger<UserProfileCreatedConsumer> logger)
    : IConsumer<UserProfileCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserProfileCreatedEvent> context)
    {
        var message = context.Message;

        var result = await notificationProvider.SendEmailAsync(
            message.Email,
            "Welcome to our platform!",
            $"Hello {message.DisplayName}, welcome to the platform! Your account has been created.");

        var log = new DeliveryLog
        {
            EventType = nameof(UserProfileCreatedEvent),
            Recipient = message.Email,
            Channel = "Email",
            Status = result.Success ? DeliveryStatus.Success : DeliveryStatus.Failed,
            Error = result.Error,
            SentAt = DateTime.UtcNow
        };

        dbContext.DeliveryLogs.Add(log);

        logger.LogInformation("Welcome email processed for {Email}", message.Email);
    }
}
