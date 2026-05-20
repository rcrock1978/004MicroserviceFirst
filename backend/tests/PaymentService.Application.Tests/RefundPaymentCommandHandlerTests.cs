using FluentAssertions;
using NSubstitute;
using PaymentService.Application.Commands;
using PaymentService.Application.Ports;
using PaymentService.Domain;
using SaaSCommon.Domain;

namespace PaymentService.Application.Tests;

public class RefundPaymentCommandHandlerTests
{
    private static TestPaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestPaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestPaymentDbContext(options);
    }

    [Fact]
    public async Task Handle_SuccessfulRefund_ShouldUpdatePaymentStatus()
    {
        await using var dbContext = CreateDbContext();
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Process("ref-123");
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var provider = Substitute.For<IPaymentProvider>();
        provider.RefundPaymentAsync("ref-123", Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(true));

        var handler = new RefundPaymentCommandHandler(dbContext, provider);
        var result = await handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await dbContext.Payments.FindAsync(payment.Id);
        updated!.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var provider = Substitute.For<IPaymentProvider>();

        var handler = new RefundPaymentCommandHandler(dbContext, provider);
        var result = await handler.Handle(new RefundPaymentCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_RefundProviderFailure_ShouldReturnConflict()
    {
        await using var dbContext = CreateDbContext();
        var payment = Payment.Create(Guid.NewGuid(), 50.00m, new TenantId(Guid.NewGuid()));
        payment.Process("ref-123");
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var provider = Substitute.For<IPaymentProvider>();
        provider.RefundPaymentAsync("ref-123", Arg.Any<CancellationToken>())
            .Returns(new PaymentResult(false, null, "gateway error"));

        var handler = new RefundPaymentCommandHandler(dbContext, provider);
        var result = await handler.Handle(new RefundPaymentCommand(payment.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }
}
