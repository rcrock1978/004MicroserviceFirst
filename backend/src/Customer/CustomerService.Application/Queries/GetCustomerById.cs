using MediatR;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Application.Queries;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<Result<Customer>>;

public sealed class GetCustomerByIdQueryHandler(ICustomerDbContext dbContext)
    : IRequestHandler<GetCustomerByIdQuery, Result<Customer>>
{
    public async Task<Result<Customer>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer is null)
        {
            return Result<Customer>.Failure(Error.NotFoundWithDetails($"Customer '{request.CustomerId}' not found."));
        }

        return Result<Customer>.Success(customer);
    }
}
