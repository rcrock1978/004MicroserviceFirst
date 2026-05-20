using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Application.Queries;

public sealed record GetCustomerOrderHistoryQuery(
    Guid CustomerId,
    string? Status = null,
    string? SortBy = null,
    bool Descending = false) : IQuery<Result<List<CustomerOrderHistory>>>;

public sealed class GetCustomerOrderHistoryQueryHandler(ICustomerDbContext dbContext)
    : IRequestHandler<GetCustomerOrderHistoryQuery, Result<List<CustomerOrderHistory>>>
{
    public async Task<Result<List<CustomerOrderHistory>>> Handle(
        GetCustomerOrderHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = CustomerOrderHistorySpecification.Apply(
            dbContext.CustomerOrderHistory,
            request.CustomerId,
            request.Status,
            request.SortBy,
            request.Descending);

        var list = await query.ToListAsync(cancellationToken);
        return Result<List<CustomerOrderHistory>>.Success(list);
    }
}
