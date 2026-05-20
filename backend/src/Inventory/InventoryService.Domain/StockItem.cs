using SaaSCommon.Domain;

namespace InventoryService.Domain;

public sealed class StockItem : Entity
{
    public Guid ProductId { get; private set; }
    public int QuantityAvailable { get; private set; }
    public int QuantityReserved { get; private set; }
    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private StockItem() { }

    public static StockItem Create(Guid productId, int initialQuantity, TenantId tenantId)
    {
        if (initialQuantity < 0)
        {
            throw new ArgumentException("Initial quantity cannot be negative.", nameof(initialQuantity));
        }

        var item = new StockItem
        {
            ProductId = productId,
            QuantityAvailable = initialQuantity,
            QuantityReserved = 0,
            TenantId = tenantId
        };

        return item;
    }

    public Reservation Reserve(Guid orderId, int quantity, TimeSpan ttl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (QuantityAvailable < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {QuantityAvailable}, Requested: {quantity}");
        }

        var reservation = Reservation.Create(orderId, Id, quantity, DateTime.UtcNow.Add(ttl), TenantId);
        _reservations.Add(reservation);
        QuantityAvailable -= quantity;
        QuantityReserved += quantity;
        SetUpdatedAt(DateTime.UtcNow);

        AddDomainEvent(new StockReserved(Id, ProductId, TenantId, orderId, quantity, reservation.ExpiresAt));
        return reservation;
    }

    public void ReleaseReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId && r.Status == ReservationStatus.Active);
        if (reservation is null)
        {
            throw new InvalidOperationException("Active reservation not found.");
        }

        reservation.Release();
        QuantityAvailable += reservation.Quantity;
        QuantityReserved -= reservation.Quantity;
        SetUpdatedAt(DateTime.UtcNow);

        AddDomainEvent(new StockReservationReleased(Id, ProductId, TenantId, reservation.OrderId, reservation.Quantity));
    }

    public void AdjustStock(int delta)
    {
        var newQuantity = QuantityAvailable + delta;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative.");
        }

        QuantityAvailable = newQuantity;
        SetUpdatedAt(DateTime.UtcNow);

        AddDomainEvent(new StockAdjusted(Id, ProductId, TenantId, delta, QuantityAvailable));
    }
}
