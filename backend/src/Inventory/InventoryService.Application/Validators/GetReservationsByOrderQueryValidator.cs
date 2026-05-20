using FluentValidation;
using InventoryService.Application.Queries;

namespace InventoryService.Application.Validators;

public sealed class GetReservationsByOrderQueryValidator : AbstractValidator<GetReservationsByOrderQuery>
{
    public GetReservationsByOrderQueryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
