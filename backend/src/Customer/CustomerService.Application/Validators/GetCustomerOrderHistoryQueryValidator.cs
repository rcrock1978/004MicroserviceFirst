using FluentValidation;
using CustomerService.Application.Queries;

namespace CustomerService.Application.Validators;

public sealed class GetCustomerOrderHistoryQueryValidator : AbstractValidator<GetCustomerOrderHistoryQuery>
{
    public GetCustomerOrderHistoryQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
