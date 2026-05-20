using FluentAssertions;
using InventoryService.Domain;
using SaaSCommon.Domain;

namespace InventoryService.Domain.Tests;

public class StockItemTests
{
    [Fact]
    public void Create_WithPositiveQuantity_ShouldSetAvailableStock()
    {
        var item = StockItem.Create(Guid.NewGuid(), 100, new TenantId(Guid.NewGuid()));

        item.QuantityAvailable.Should().Be(100);
        item.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldThrow()
    {
        Action act = () => StockItem.Create(Guid.NewGuid(), -1, new TenantId(Guid.NewGuid()));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reserve_ShouldDecreaseAvailableAndIncreaseReserved()
    {
        var item = StockItem.Create(Guid.NewGuid(), 100, new TenantId(Guid.NewGuid()));

        var reservation = item.Reserve(Guid.NewGuid(), 10, TimeSpan.FromMinutes(10));

        item.QuantityAvailable.Should().Be(90);
        item.QuantityReserved.Should().Be(10);
        reservation.Status.Should().Be(ReservationStatus.Active);
    }

    [Fact]
    public void Reserve_InsufficientStock_ShouldThrow()
    {
        var item = StockItem.Create(Guid.NewGuid(), 5, new TenantId(Guid.NewGuid()));

        Action act = () => item.Reserve(Guid.NewGuid(), 10, TimeSpan.FromMinutes(10));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reserve_ShouldRaiseStockReservedEvent()
    {
        var item = StockItem.Create(Guid.NewGuid(), 100, new TenantId(Guid.NewGuid()));

        item.Reserve(Guid.NewGuid(), 10, TimeSpan.FromMinutes(10));

        item.DomainEvents.Should().ContainSingle(e => e is StockReserved);
    }

    [Fact]
    public void ReleaseReservation_ShouldRestoreStock()
    {
        var item = StockItem.Create(Guid.NewGuid(), 100, new TenantId(Guid.NewGuid()));
        var reservation = item.Reserve(Guid.NewGuid(), 10, TimeSpan.FromMinutes(10));
        item.ClearDomainEvents();

        item.ReleaseReservation(reservation.Id);

        item.QuantityAvailable.Should().Be(100);
        item.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public void AdjustStock_ShouldUpdateAvailableQuantity()
    {
        var item = StockItem.Create(Guid.NewGuid(), 50, new TenantId(Guid.NewGuid()));
        item.ClearDomainEvents();

        item.AdjustStock(25);

        item.QuantityAvailable.Should().Be(75);
        item.DomainEvents.Should().ContainSingle(e => e is StockAdjusted);
    }

    [Fact]
    public void AdjustStock_BelowZero_ShouldThrow()
    {
        var item = StockItem.Create(Guid.NewGuid(), 10, new TenantId(Guid.NewGuid()));

        Action act = () => item.AdjustStock(-15);
        act.Should().Throw<InvalidOperationException>();
    }
}
