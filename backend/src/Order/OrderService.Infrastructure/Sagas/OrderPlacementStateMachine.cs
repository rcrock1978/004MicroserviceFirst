using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure;
using InventoryService.Contracts;
using PaymentService.Contracts;
using OrderService.Contracts;

namespace OrderService.Infrastructure.Sagas;

public sealed class OrderPlacementStateMachine : MassTransitStateMachine<OrderPlacementSagaState>
{
    public State Placed { get; private set; } = null!;
    public State AwaitingPayment { get; private set; } = null!;
    public State Paid { get; private set; } = null!;
    public State PaymentFailed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<OrderPlacedEvent> OrderPlaced { get; private set; } = null!;
    public Event<StockReservedEvent> StockReserved { get; private set; } = null!;
    public Event<PaymentProcessedEvent> PaymentProcessed { get; private set; } = null!;
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; private set; } = null!;
    public Event<StockReservationFailedEvent> StockReservationFailed { get; private set; } = null!;

    public OrderPlacementStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReserved, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentProcessed, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => StockReservationFailed, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.CustomerId = ctx.Message.CustomerId;
                    ctx.Saga.TenantId = ctx.Message.TenantId;
                    ctx.Saga.TotalAmount = ctx.Message.TotalAmount;
                })
                .TransitionTo(Placed));

        During(Placed,
            When(StockReserved)
                .Publish(ctx => new OrderPaymentRequestedEvent(
                    ctx.Saga.OrderId,
                    ctx.Saga.CustomerId,
                    ctx.Saga.TenantId,
                    ctx.Saga.TotalAmount,
                    DateTime.UtcNow))
                .TransitionTo(AwaitingPayment),
            When(StockReservationFailed)
                .ThenAsync(async ctx =>
                {
                    var provider = ctx.GetPayload<IServiceProvider>();
                    var dbContext = provider.GetRequiredService<OrderDbContext>();
                    var order = await dbContext.Orders.FindAsync(ctx.Saga.OrderId, CancellationToken.None);
                    if (order is not null)
                    {
                        order.Cancel();
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                })
                .TransitionTo(Cancelled));

        During(AwaitingPayment,
            When(PaymentProcessed)
                .ThenAsync(async ctx =>
                {
                    var provider = ctx.GetPayload<IServiceProvider>();
                    var dbContext = provider.GetRequiredService<OrderDbContext>();
                    var order = await dbContext.Orders.FindAsync(ctx.Saga.OrderId, CancellationToken.None);
                    if (order is not null)
                    {
                        order.MarkAsPaid(ctx.Message.ProviderReference);
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                })
                .TransitionTo(Paid),
            When(PaymentFailedEvent)
                .ThenAsync(async ctx =>
                {
                    var provider = ctx.GetPayload<IServiceProvider>();
                    var dbContext = provider.GetRequiredService<OrderDbContext>();
                    var order = await dbContext.Orders.FindAsync(ctx.Saga.OrderId, CancellationToken.None);
                    if (order is not null)
                    {
                        order.MarkPaymentFailed();
                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                })
                .TransitionTo(PaymentFailed));
    }
}
