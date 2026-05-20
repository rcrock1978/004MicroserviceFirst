using SaaSCommon.Domain;

namespace TenantService.Domain;

public sealed class Tenant : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public TenantConfiguration Configuration { get; private set; } = new();
    private readonly List<FeatureFlag> _featureFlags = new();
    public IReadOnlyCollection<FeatureFlag> FeatureFlags => _featureFlags.AsReadOnly();
    public TenantStatus Status { get; private set; } = TenantStatus.Pending;

    private Tenant() { }

    public static Tenant Provision(string name, string slug, TenantConfiguration? configuration = null)
    {
        var tenant = new Tenant
        {
            Name = name,
            Slug = slug,
            Configuration = configuration ?? new TenantConfiguration(),
            Status = TenantStatus.Pending
        };

        tenant.TenantId = new TenantId(tenant.Id);

        tenant.AddDomainEvent(new TenantProvisioned(tenant.Id, tenant.Name, tenant.Slug));
        return tenant;
    }

    public void UpdateConfiguration(TenantConfiguration configuration)
    {
        Configuration = configuration;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new TenantConfigurationUpdated(Id, configuration));
    }

    public void EnableFeatureFlag(string key)
    {
        var existing = _featureFlags.FirstOrDefault(f => f.Key == key);
        if (existing is not null)
        {
            _featureFlags.Remove(existing);
        }

        _featureFlags.Add(new FeatureFlag(key, true, existing?.Description));
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new FeatureFlagToggled(Id, key, true));
    }

    public void DisableFeatureFlag(string key)
    {
        var existing = _featureFlags.FirstOrDefault(f => f.Key == key);
        if (existing is not null)
        {
            _featureFlags.Remove(existing);
        }

        _featureFlags.Add(new FeatureFlag(key, false, existing?.Description));
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new FeatureFlagToggled(Id, key, false));
    }

    public void Activate()
    {
        if (Status == TenantStatus.Active)
        {
            return;
        }

        Status = TenantStatus.Active;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new TenantActivated(Id));
    }

    public void Deactivate()
    {
        if (Status == TenantStatus.Deactivated)
        {
            return;
        }

        Status = TenantStatus.Deactivated;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new TenantDeactivated(Id));
    }
}
