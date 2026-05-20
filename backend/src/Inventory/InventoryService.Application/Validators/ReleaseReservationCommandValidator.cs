using FluentValidation;
using InventoryService.Application.Commands;

namespace InventoryService.Application.Validators;

public sealed class ReleaseReservationCommandValidator : AbstractValidator<ReleaseReservationCommand>
{
    public ReleaseReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
    }
}
