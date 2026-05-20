using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Application;

public interface IOrderDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
