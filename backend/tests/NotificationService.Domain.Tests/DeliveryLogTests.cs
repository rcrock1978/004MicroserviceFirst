using FluentAssertions;
using NotificationService.Domain;

namespace NotificationService.Domain.Tests;

public class DeliveryLogTests
{
    [Fact]
    public void DeliveryLog_ShouldHaveGeneratedId()
    {
        var log = new DeliveryLog();

        log.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DeliveryLog_ShouldDefaultToPendingLikeState()
    {
        var log = new DeliveryLog();

        log.Status.Should().Be(default(DeliveryStatus));
    }

    [Fact]
    public void DeliveryResult_Success_ShouldBeTrue()
    {
        var result = new DeliveryResult(true);

        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void DeliveryResult_Failure_ShouldContainError()
    {
        var result = new DeliveryResult(false, "smtp error");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("smtp error");
    }
}
