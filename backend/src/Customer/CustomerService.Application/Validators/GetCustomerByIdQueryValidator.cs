using FluentValidation;
using CustomerService.Application.Queries;

namespace CustomerService.Application.Validators;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
