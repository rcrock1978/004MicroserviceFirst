using FluentAssertions;
using NSubstitute;
using NotificationService.Application.Ports;
using NotificationService.Domain;

namespace NotificationService.Application.Tests;

public class NotificationProviderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldReturnSuccess()
    {
        var provider = Substitute.For<INotificationProvider>();
        provider.SendEmailAsync("to@example.com", "subject", "body")
            .Returns(new DeliveryResult(true));

        var result = await provider.SendEmailAsync("to@example.com", "subject", "body");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnFailure()
    {
        var provider = Substitute.For<INotificationProvider>();
        provider.SendSmsAsync("+1234567890", "message")
            .Returns(new DeliveryResult(false, "invalid number"));

        var result = await provider.SendSmsAsync("+1234567890", "message");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("invalid number");
    }
}
