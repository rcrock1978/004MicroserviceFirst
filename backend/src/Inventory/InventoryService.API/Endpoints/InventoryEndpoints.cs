using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaSCommon.Domain;
using InventoryService.Application.Commands;
using InventoryService.Application.Queries;

namespace InventoryService.API.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory");

        group.MapGet("/{productId:guid}", async (Guid productId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetStockByProductQuery(productId));
            return result.Match(
                onSuccess: stock => Results.Ok(stock),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/reservations/order/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetReservationsByOrderQuery(orderId));
            return result.Match(
                onSuccess: reservations => Results.Ok(reservations),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{productId:guid}/adjust", async (Guid productId, [FromBody] AdjustStockRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new AdjustStockCommand(productId, request.Delta, request.TenantId));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
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

public sealed record AdjustStockRequest(int Delta, Guid TenantId);
