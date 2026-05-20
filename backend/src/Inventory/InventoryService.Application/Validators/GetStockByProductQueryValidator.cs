using FluentValidation;
using InventoryService.Application.Queries;

namespace InventoryService.Application.Validators;

public sealed class GetStockByProductQueryValidator : AbstractValidator<GetStockByProductQuery>
{
    public GetStockByProductQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
