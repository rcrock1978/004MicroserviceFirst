namespace SaaSCommon.Domain;

public sealed record TenantId(Guid Value)
{
    public static TenantId Empty => new(Guid.Empty);
}
