using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Queries;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Application.Queries;

public sealed record GetCustomerByEmailQuery(string Email) : IQuery<Result<Customer>>;

public sealed class GetCustomerByEmailQueryHandler(ICustomerDbContext dbContext)
    : IRequestHandler<GetCustomerByEmailQuery, Result<Customer>>
{
    public async Task<Result<Customer>> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

        if (customer is null)
        {
            return Result<Customer>.Failure(Error.NotFoundWithDetails($"Customer with email '{request.Email}' not found."));
        }

        return Result<Customer>.Success(customer);
    }
}
