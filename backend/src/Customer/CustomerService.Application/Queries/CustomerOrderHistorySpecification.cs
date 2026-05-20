using CustomerService.Domain;

namespace CustomerService.Application.Queries;

public static class CustomerOrderHistorySpecification
{
    public static IQueryable<CustomerOrderHistory> Apply(
        IQueryable<CustomerOrderHistory> query,
        Guid customerId,
        string? status,
        string? sortBy,
        bool descending)
    {
        query = query.Where(h => h.CustomerId == customerId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(h => h.Status == status);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "orderdate" => descending ? query.OrderByDescending(h => h.OrderDate) : query.OrderBy(h => h.OrderDate),
            "status" => descending ? query.OrderByDescending(h => h.Status) : query.OrderBy(h => h.Status),
            _ => query.OrderByDescending(h => h.OrderDate)
        };

        return query;
    }
}
