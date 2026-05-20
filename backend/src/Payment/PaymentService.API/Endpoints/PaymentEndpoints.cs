using MediatR;
using SaaSCommon.Domain;
using PaymentService.Application.Queries;

namespace PaymentService.API.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments");

        group.MapGet("/order/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPaymentByOrderIdQuery(orderId));
            return result.Match(
                onSuccess: payment => Results.Ok(payment),
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
