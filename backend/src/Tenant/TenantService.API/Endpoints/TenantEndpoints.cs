using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaSCommon.Domain;
using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Domain;

namespace TenantService.API.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants");

        group.MapPost("/", async (ProvisionTenantCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Match(
                onSuccess: id => Results.Created($"/api/tenants/{id}", id),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTenantByIdQuery(id));
            return result.Match(
                onSuccess: tenant => Results.Ok(tenant),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapGet("/by-slug/{slug}", async (string slug, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTenantBySlugQuery(slug));
            return result.Match(
                onSuccess: tenant => Results.Ok(tenant),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPut("/{id:guid}/configuration", async (Guid id, [FromBody] TenantConfiguration configuration, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateTenantConfigurationCommand(id, configuration));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/features/{key}/enable", async (Guid id, string key, IMediator mediator) =>
        {
            var result = await mediator.Send(new EnableFeatureFlagCommand(id, key));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/features/{key}/disable", async (Guid id, string key, IMediator mediator) =>
        {
            var result = await mediator.Send(new DisableFeatureFlagCommand(id, key));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateTenantCommand(id));
            return result.Match(
                onSuccess: _ => Results.NoContent(),
                onFailure: error => MapErrorToResult(error));
        });

        group.MapPost("/{id:guid}/deactivate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeactivateTenantCommand(id));
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
