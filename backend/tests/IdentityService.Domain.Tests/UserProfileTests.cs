using FluentAssertions;
using IdentityService.Domain;
using SaaSCommon.Domain;

namespace IdentityService.Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveUser()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");

        user.IsActive.Should().BeTrue();
        user.Email.Should().Be("user@example.com");
        user.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Constructor_ShouldRaiseUserProfileCreatedEvent()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");

        user.DomainEvents.Should().ContainSingle(e => e is UserProfileCreated);
    }

    [Fact]
    public void UpdateClaims_ShouldRaiseUserProfileUpdatedEvent()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        user.ClearDomainEvents();

        user.UpdateClaims(new Dictionary<string, string> { { "key", "value" } });

        user.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdated);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        user.ClearDomainEvents();

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaiseUserProfileDeactivatedEvent()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        user.ClearDomainEvents();

        user.Deactivate();

        user.DomainEvents.Should().ContainSingle(e => e is UserProfileDeactivated);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotRaiseEvent()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        user.Deactivate();
        user.ClearDomainEvents();

        user.Deactivate();

        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateFromIdP_ShouldUpdateEmailAndDisplayName()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "old@example.com", "Old Name");
        user.ClearDomainEvents();

        user.UpdateFromIdP("new@example.com", "New Name", new List<string>(), new Dictionary<string, string>());

        user.Email.Should().Be("new@example.com");
        user.DisplayName.Should().Be("New Name");
        user.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdated);
    }
}
