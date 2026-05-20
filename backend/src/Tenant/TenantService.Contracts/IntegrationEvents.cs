namespace TenantService.Contracts;

public sealed record TenantProvisionedEvent(Guid TenantId, string Name, string Slug, DateTime OccurredOn);
public sealed record TenantActivatedEvent(Guid TenantId, DateTime OccurredOn);
public sealed record TenantDeactivatedEvent(Guid TenantId, DateTime OccurredOn);
public sealed record TenantConfigurationUpdatedEvent(Guid TenantId, Dictionary<string, string> Settings, DateTime OccurredOn);
public sealed record FeatureFlagToggledEvent(Guid TenantId, string Key, bool Enabled, DateTime OccurredOn);
