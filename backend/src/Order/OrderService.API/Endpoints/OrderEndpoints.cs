using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.Queries;
using SaaSCommon.Domain;

namespace OrderService.API.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders");

        group.MapPost("/", async (CreateOrderCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Match(
                onSuccess: id => Results.Created($"/api/orders/{id}", id),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/place", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new PlaceOrderCommand(id));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelOrderCommand(id));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/ship", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new MarkOrderAsShippedCommand(id));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id));
            return result.Match(
                onSuccess: order => Results.Ok(order),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/", async ([FromQuery] Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrdersByCustomerQuery(customerId));
            return result.Match(
                onSuccess: orders => Results.Ok(orders),
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
