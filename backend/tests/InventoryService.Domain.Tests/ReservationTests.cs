using FluentAssertions;
using InventoryService.Domain;
using SaaSCommon.Domain;

namespace InventoryService.Domain.Tests;

public class ReservationTests
{
    [Fact]
    public void Create_ShouldSetActiveStatus()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 5, DateTime.UtcNow.AddMinutes(10), new TenantId(Guid.NewGuid()));

        reservation.Status.Should().Be(ReservationStatus.Active);
        reservation.Quantity.Should().Be(5);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrow()
    {
        Action act = () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 0, DateTime.UtcNow, new TenantId(Guid.NewGuid()));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Release_ShouldSetStatusToReleased()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 5, DateTime.UtcNow.AddMinutes(10), new TenantId(Guid.NewGuid()));

        reservation.Release();

        reservation.Status.Should().Be(ReservationStatus.Released);
    }

    [Fact]
    public void Release_WhenNotActive_ShouldThrow()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 5, DateTime.UtcNow.AddMinutes(10), new TenantId(Guid.NewGuid()));
        reservation.Release();

        Action act = () => reservation.Release();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Expire_ShouldSetStatusToExpired()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 5, DateTime.UtcNow.AddMinutes(10), new TenantId(Guid.NewGuid()));

        reservation.Expire();

        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Commit_ShouldSetStatusToCommitted()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 5, DateTime.UtcNow.AddMinutes(10), new TenantId(Guid.NewGuid()));

        reservation.Commit();

        reservation.Status.Should().Be(ReservationStatus.Committed);
    }
}
