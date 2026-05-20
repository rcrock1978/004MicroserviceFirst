using FluentAssertions;
using SaaSCommon.Domain;
using TenantService.Domain;

namespace TenantService.Domain.Tests;

public class TenantTests
{
    [Fact]
    public void Provision_ShouldCreateTenantWithPendingStatus()
    {
        var tenant = Tenant.Provision("Acme", "acme");

        tenant.Name.Should().Be("Acme");
        tenant.Slug.Should().Be("acme");
        tenant.Status.Should().Be(TenantStatus.Pending);
    }

    [Fact]
    public void Provision_ShouldRaiseTenantProvisionedEvent()
    {
        var tenant = Tenant.Provision("Acme", "acme");

        tenant.DomainEvents.Should().ContainSingle(e => e is TenantProvisioned);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.ClearDomainEvents();

        tenant.Activate();

        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Activate_ShouldRaiseTenantActivatedEvent()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.ClearDomainEvents();

        tenant.Activate();

        tenant.DomainEvents.Should().ContainSingle(e => e is TenantActivated);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotRaiseEvent()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.Activate();
        tenant.ClearDomainEvents();

        tenant.Activate();

        tenant.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToDeactivated()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.Activate();
        tenant.ClearDomainEvents();

        tenant.Deactivate();

        tenant.Status.Should().Be(TenantStatus.Deactivated);
    }

    [Fact]
    public void EnableFeatureFlag_ShouldAddFlag()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.ClearDomainEvents();

        tenant.EnableFeatureFlag("feature-a");

        tenant.FeatureFlags.Should().ContainSingle(f => f.Key == "feature-a" && f.Enabled);
        tenant.DomainEvents.Should().ContainSingle(e => e is FeatureFlagToggled);
    }

    [Fact]
    public void DisableFeatureFlag_ShouldDisableFlag()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.EnableFeatureFlag("feature-a");
        tenant.ClearDomainEvents();

        tenant.DisableFeatureFlag("feature-a");

        tenant.FeatureFlags.Should().ContainSingle(f => f.Key == "feature-a" && !f.Enabled);
    }

    [Fact]
    public void UpdateConfiguration_ShouldRaiseConfigurationUpdatedEvent()
    {
        var tenant = Tenant.Provision("Acme", "acme");
        tenant.ClearDomainEvents();
        var config = new TenantConfiguration { MaxUsers = 100 };

        tenant.UpdateConfiguration(config);

        tenant.Configuration.MaxUsers.Should().Be(100);
        tenant.DomainEvents.Should().ContainSingle(e => e is TenantConfigurationUpdated);
    }
}
