using FluentAssertions;
using OrderService.Domain;
using SaaSCommon.Domain;

namespace OrderService.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Create_ShouldSetDraftStatus()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));

        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Create_ShouldRaiseOrderCreatedEvent()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));

        order.DomainEvents.Should().ContainSingle(e => e is OrderCreated);
    }

    [Fact]
    public void AddItem_ShouldIncreaseTotalAmount()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 2, 10.00m);

        order.TotalAmount.Should().Be(20.00m);
        order.Items.Should().ContainSingle();
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));

        Action act = () => order.AddItem(Guid.NewGuid(), 0, 10.00m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_WhenNotDraft_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();

        Action act = () => order.AddItem(Guid.NewGuid(), 1, 10.00m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Place_ShouldSetStatusToPlaced()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);

        order.Place();

        order.Status.Should().Be(OrderStatus.Placed);
    }

    [Fact]
    public void Place_WithoutItems_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));

        Action act = () => order.Place();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsPaid_ShouldSetStatusToPaid()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();
        order.MarkPaymentRequested();

        order.MarkAsPaid("ref-123");

        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentProviderReference.Should().Be("ref-123");
    }

    [Fact]
    public void MarkAsPaid_WhenNotPaymentPending_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();

        Action act = () => order.MarkAsPaid("ref-123");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();
        order.MarkPaymentRequested();
        order.MarkAsPaid("ref-123");

        Action act = () => order.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkPaymentFailed_ShouldSetStatusToPaymentFailed()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();
        order.MarkPaymentRequested();

        order.MarkPaymentFailed();

        order.Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [Fact]
    public void MarkAsShipped_ShouldSetStatusToShipped()
    {
        var order = Order.Create(Guid.NewGuid(), new TenantId(Guid.NewGuid()));
        order.AddItem(Guid.NewGuid(), 1, 10.00m);
        order.Place();
        order.MarkPaymentRequested();
        order.MarkAsPaid("ref-123");

        order.MarkAsShipped();

        order.Status.Should().Be(OrderStatus.Shipped);
    }
}
