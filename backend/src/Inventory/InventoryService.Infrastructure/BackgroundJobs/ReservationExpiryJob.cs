using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using InventoryService.Application.Commands;

namespace InventoryService.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class ReservationExpiryJob : IJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationExpiryJob> _logger;

    public ReservationExpiryJob(IMediator mediator, ILogger<ReservationExpiryJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running reservation expiry job");
        var result = await _mediator.Send(new ExpireReservationsCommand(), context.CancellationToken);
        _logger.LogInformation("Expired {Count} reservations", result.IsSuccess ? result.Value : 0);
    }
}
