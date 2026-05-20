namespace SaaSCommon.Domain;

public abstract record DomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
