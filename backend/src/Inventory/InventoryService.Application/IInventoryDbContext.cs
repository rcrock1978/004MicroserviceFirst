using Microsoft.EntityFrameworkCore;
using InventoryService.Domain;

namespace InventoryService.Application;

public interface IInventoryDbContext
{
    DbSet<StockItem> StockItems { get; }
    DbSet<Reservation> Reservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
