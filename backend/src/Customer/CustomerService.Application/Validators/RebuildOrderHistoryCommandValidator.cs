using FluentValidation;
using CustomerService.Application.Commands;

namespace CustomerService.Application.Validators;

public sealed class RebuildOrderHistoryCommandValidator : AbstractValidator<RebuildOrderHistoryCommand>
{
    public RebuildOrderHistoryCommandValidator()
    {
    }
}
