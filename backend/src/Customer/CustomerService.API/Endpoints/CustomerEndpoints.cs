using MediatR;
using SaaSCommon.Domain;
using CustomerService.Application.Commands;
using CustomerService.Application.Queries;
using CustomerService.Domain;

namespace CustomerService.API.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers");

        group.MapPost("/", async (CreateCustomerCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Match(
                onSuccess: id => Results.Created($"/api/customers/{id}", id),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerByIdQuery(id));
            return result.Match(
                onSuccess: customer => Results.Ok(customer),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/by-email/{email}", async (string email, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerByEmailQuery(email));
            return result.Match(
                onSuccess: customer => Results.Ok(customer),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/{id:guid}/order-history", async (
            Guid id,
            string? status,
            string? sortBy,
            bool descending = false,
            IMediator mediator = default!) =>
        {
            var result = await mediator.Send(new GetCustomerOrderHistoryQuery(id, status, sortBy, descending));
            return result.Match(
                onSuccess: history => Results.Ok(history),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/order-history/rebuild", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new RebuildOrderHistoryCommand());
            return result.Match(
                onSuccess: _ => Results.Accepted(),
                onFailure: error => MapErrorToResult(error));
        });

        return app;
    }

    private static IResult MapErrorToResult(Error error)
    {
        return error.Code switch
        {
            "Error.NotFound" => Results.NotFound(new { error.Code, error.Message, error.Details }),
            "Error.Conflict" => Results.Conflict(new { error.Code, error.Message, error.Details }),
            "Error.Validation" => Results.BadRequest(new { error.Code, error.Message, error.Details }),
            "Error.Unauthorized" => Results.StatusCode(403),
            _ => Results.BadRequest(new { error.Code, error.Message, error.Details })
        };
    }
}
