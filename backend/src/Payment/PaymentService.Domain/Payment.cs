using SaaSCommon.Domain;

namespace PaymentService.Domain;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Refunded
}

public sealed class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? ProviderReference { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount, TenantId tenantId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        }

        return new Payment
        {
            OrderId = orderId,
            Amount = amount,
            TenantId = tenantId,
            Status = PaymentStatus.Pending
        };
    }

    public void Process(string providerReference)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException("Payment can only be processed when pending or processing.");
        }

        Status = PaymentStatus.Succeeded;
        ProviderReference = providerReference;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new PaymentProcessed(Id, OrderId, TenantId, Amount, providerReference));
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Processing)
        {
            throw new InvalidOperationException("Payment can only fail when pending or processing.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new PaymentFailed(Id, OrderId, TenantId, Amount, reason));
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Only succeeded payments can be refunded.");
        }

        Status = PaymentStatus.Refunded;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new PaymentRefunded(Id, OrderId, TenantId, Amount, ProviderReference));
    }
}
