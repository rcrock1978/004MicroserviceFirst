using SaaSCommon.Domain;

namespace TenantService.Domain;

public sealed record TenantProvisioned(Guid TenantId, string Name, string Slug) : DomainEvent;
public sealed record TenantActivated(Guid TenantId) : DomainEvent;
public sealed record TenantDeactivated(Guid TenantId) : DomainEvent;
public sealed record TenantConfigurationUpdated(Guid TenantId, TenantConfiguration Configuration) : DomainEvent;
public sealed record FeatureFlagToggled(Guid TenantId, string Key, bool Enabled) : DomainEvent;
