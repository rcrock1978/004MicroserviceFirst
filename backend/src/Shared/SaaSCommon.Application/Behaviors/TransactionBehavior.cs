using System.Transactions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SaaSCommon.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (System.Transactions.Transaction.Current is not null)
        {
            return await next();
        }

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = (System.Transactions.IsolationLevel)System.Data.IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var response = await next();
            transactionScope.Complete();
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction failed for {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
