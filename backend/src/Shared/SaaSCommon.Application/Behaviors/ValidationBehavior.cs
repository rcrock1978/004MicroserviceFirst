using FluentValidation;
using MediatR;
using SaaSCommon.Domain;

namespace SaaSCommon.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Any())
        {
            var errorDetails = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = Error.Validation with { Details = errorDetails };

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(TResponse).GetMethod("Failure");
                if (failureMethod is not null)
                {
                    return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
                }
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
