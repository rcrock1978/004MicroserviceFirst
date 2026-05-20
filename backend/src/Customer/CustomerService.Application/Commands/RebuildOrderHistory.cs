using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using CustomerService.Domain;

namespace CustomerService.Application.Commands;

public sealed record RebuildOrderHistoryCommand : ICommand<Result<object>>;

public sealed class RebuildOrderHistoryCommandHandler(ICustomerDbContext dbContext)
    : IRequestHandler<RebuildOrderHistoryCommand, Result<object>>
{
    public async Task<Result<object>> Handle(RebuildOrderHistoryCommand request, CancellationToken cancellationToken)
    {
        var histories = await dbContext.CustomerOrderHistory.ToListAsync(cancellationToken);
        dbContext.CustomerOrderHistory.RemoveRange(histories);
        await dbContext.SaveChangesAsync(cancellationToken);

        // TODO: Replay order events from a dedicated event store or outbox to rebuild projections.
        // For now, the projection is cleared and will be rebuilt as new events arrive.

        return Result.Success();
    }
}
