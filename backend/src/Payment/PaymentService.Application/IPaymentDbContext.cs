using Microsoft.EntityFrameworkCore;
using PaymentService.Domain;

namespace PaymentService.Application;

public interface IPaymentDbContext
{
    DbSet<Payment> Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
