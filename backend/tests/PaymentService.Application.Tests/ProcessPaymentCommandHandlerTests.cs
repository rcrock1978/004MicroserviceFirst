using FluentAssertions;
using NSubstitute;
using PaymentService.Application.Commands;
using PaymentService.Application.Ports;
using SaaSCommon.Domain;

namespace PaymentService.Application.Tests;

public class ProcessPaymentCommandHandlerTests
{
    private static TestPaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestPaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_SuccessfulProviderResponse_ShouldProcessPayment()
    {
        await using var dbContext = CreateDbContext();
        var provider = Substitute.For<IPaymentProvider>();
        provider.ProcessPaymentAsync(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(true, "ref-123"));

        var handler = new ProcessPaymentCommandHandler(dbContext, provider);
        var result = await handler.Handle(new ProcessPaymentCommand(Guid.NewGuid(), 100.00m, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var payment = dbContext.Payments.First();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Handle_FailedProviderResponse_ShouldFailPayment()
    {
        await using var dbContext = CreateDbContext();
        var provider = Substitute.For<IPaymentProvider>();
        provider.ProcessPaymentAsync(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(false, null, "card declined"));

        var handler = new ProcessPaymentCommandHandler(dbContext, provider);
        var result = await handler.Handle(new ProcessPaymentCommand(Guid.NewGuid(), 100.00m, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var payment = dbContext.Payments.First();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("card declined");
    }
}
