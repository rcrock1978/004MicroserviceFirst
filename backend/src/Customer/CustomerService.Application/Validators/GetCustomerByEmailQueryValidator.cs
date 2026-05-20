using FluentValidation;
using CustomerService.Application.Queries;

namespace CustomerService.Application.Validators;

public sealed class GetCustomerByEmailQueryValidator : AbstractValidator<GetCustomerByEmailQuery>
{
    public GetCustomerByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
    }
}
