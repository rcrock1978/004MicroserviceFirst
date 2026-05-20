using FluentValidation;
using InventoryService.Application.Commands;

namespace InventoryService.Application.Validators;

public sealed class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
}
