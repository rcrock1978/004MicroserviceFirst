namespace InventoryService.Contracts;

public sealed record StockReservedEvent(Guid StockItemId, Guid ProductId, Guid TenantId, Guid OrderId, int Quantity, DateTime ExpiresAt, DateTime OccurredOn);
public sealed record StockReservationReleasedEvent(Guid StockItemId, Guid ProductId, Guid TenantId, Guid OrderId, int Quantity, DateTime OccurredOn);
public sealed record StockReservationExpiredEvent(Guid StockItemId, Guid ProductId, Guid TenantId, Guid OrderId, int Quantity, DateTime OccurredOn);
public sealed record StockAdjustedEvent(Guid StockItemId, Guid ProductId, Guid TenantId, int Delta, int NewAvailable, DateTime OccurredOn);
public sealed record StockReservationFailedEvent(Guid ProductId, Guid TenantId, Guid OrderId, int Quantity, string Reason, DateTime OccurredOn);
