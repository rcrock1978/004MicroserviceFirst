using SaaSCommon.Domain;

namespace InventoryService.Domain;

public enum ReservationStatus
{
    Active,
    Released,
    Expired,
    Committed
}

public sealed class Reservation : Entity
{
    public Guid OrderId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Active;
    public Guid StockItemId { get; private set; }
    public StockItem? StockItem { get; private set; }

    private Reservation() { }

    public static Reservation Create(Guid orderId, Guid stockItemId, int quantity, DateTime expiresAt, TenantId tenantId)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        return new Reservation
        {
            OrderId = orderId,
            StockItemId = stockItemId,
            Quantity = quantity,
            ExpiresAt = expiresAt,
            TenantId = tenantId,
            Status = ReservationStatus.Active
        };
    }

    public void Release()
    {
        if (Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException("Only active reservations can be released.");
        }

        Status = ReservationStatus.Released;
        SetUpdatedAt(DateTime.UtcNow);
    }

    public void Expire()
    {
        if (Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException("Only active reservations can be expired.");
        }

        Status = ReservationStatus.Expired;
        SetUpdatedAt(DateTime.UtcNow);
    }

    public void Commit()
    {
        if (Status != ReservationStatus.Active)
        {
            throw new InvalidOperationException("Only active reservations can be committed.");
        }

        Status = ReservationStatus.Committed;
        SetUpdatedAt(DateTime.UtcNow);
    }
}
