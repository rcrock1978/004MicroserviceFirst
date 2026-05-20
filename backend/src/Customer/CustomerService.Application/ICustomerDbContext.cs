using Microsoft.EntityFrameworkCore;
using CustomerService.Domain;

namespace CustomerService.Application;

public interface ICustomerDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<CustomerOrderHistory> CustomerOrderHistory { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
