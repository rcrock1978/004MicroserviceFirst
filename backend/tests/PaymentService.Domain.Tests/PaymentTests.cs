using FluentAssertions;
using PaymentService.Domain;
using SaaSCommon.Domain;

namespace PaymentService.Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Create_WithPositiveAmount_ShouldCreatePendingPayment()
    {
        var payment = Payment.Create(Guid.NewGuid(), 100.00m, new TenantId(Guid.NewGuid()));

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Should().Be(100.00m);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        Action act = () => Payment.Create(Guid.NewGuid(), 0, new TenantId(Guid.NewGuid()));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldSetStatusToSucceeded()
    {
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Process("ref-123");

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.ProviderReference.Should().Be("ref-123");
        payment.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Fail_ShouldSetStatusToFailed()
    {
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Fail("insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("insufficient funds");
    }

    [Fact]
    public void Refund_ShouldSetStatusToRefunded()
    {
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Process("ref-123");
        payment.Refund();

        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_WhenNotSucceeded_ShouldThrow()
    {
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Fail("error");

        Action act = () => payment.Refund();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Process_ShouldRaisePaymentProcessedEvent()
    {
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Process("ref-123");

        payment.DomainEvents.Should().ContainSingle(e => e is PaymentProcessed);
    }
}
