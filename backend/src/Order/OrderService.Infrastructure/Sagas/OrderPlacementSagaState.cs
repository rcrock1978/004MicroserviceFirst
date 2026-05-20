using MassTransit;
using OrderService.Domain;
using OrderService.Infrastructure;

namespace OrderService.Infrastructure.Sagas;

public sealed class OrderPlacementSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
    public decimal TotalAmount { get; set; }
}
