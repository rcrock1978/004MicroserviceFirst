using MassTransit;
using MediatR;
using OrderService.Contracts;
using PaymentService.Application.Commands;

namespace PaymentService.Infrastructure.Consumers;

public sealed class OrderPaymentRequestedConsumer(IMediator mediator) : IConsumer<OrderPaymentRequestedEvent>
{
    public async Task Consume(ConsumeContext<OrderPaymentRequestedEvent> context)
    {
        var message = context.Message;
        await mediator.Send(new ProcessPaymentCommand(message.OrderId, message.Amount, message.TenantId));
    }
}
