using SaaSCommon.Domain;

namespace InventoryService.Domain;

public sealed record StockReserved(Guid StockItemId, Guid ProductId, TenantId TenantId, Guid OrderId, int Quantity, DateTime ExpiresAt) : DomainEvent;
public sealed record StockReservationReleased(Guid StockItemId, Guid ProductId, TenantId TenantId, Guid OrderId, int Quantity) : DomainEvent;
public sealed record StockReservationExpired(Guid StockItemId, Guid ProductId, TenantId TenantId, Guid OrderId, int Quantity) : DomainEvent;
public sealed record StockAdjusted(Guid StockItemId, Guid ProductId, TenantId TenantId, int Delta, int NewAvailable) : DomainEvent;
