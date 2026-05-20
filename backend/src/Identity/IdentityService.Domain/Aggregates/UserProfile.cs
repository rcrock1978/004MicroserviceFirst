using SaaSCommon.Domain;

namespace IdentityService.Domain;

public class UserProfile : Entity
{
    public string ExternalId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public List<string> Roles { get; private set; } = new();
    public Dictionary<string, string> Claims { get; private set; } = new();
    public bool IsActive { get; private set; } = true;

    private UserProfile() { }

    public UserProfile(
        TenantId tenantId,
        string externalId,
        string email,
        string displayName,
        List<string>? roles = null,
        Dictionary<string, string>? claims = null)
    {
        TenantId = tenantId;
        ExternalId = externalId;
        Email = email;
        DisplayName = displayName;
        Roles = roles ?? new List<string>();
        Claims = claims ?? new Dictionary<string, string>();
        IsActive = true;

        AddDomainEvent(new UserProfileCreated(
            Id, TenantId, ExternalId, Email, DisplayName, Roles, Claims));
    }

    public void UpdateClaims(Dictionary<string, string> claims)
    {
        Claims = new Dictionary<string, string>(claims);
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new UserProfileUpdated(
            Id, TenantId, ExternalId, Email, DisplayName, Roles, Claims));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new UserProfileDeactivated(Id, TenantId, ExternalId));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new UserProfileUpdated(
            Id, TenantId, ExternalId, Email, DisplayName, Roles, Claims));
    }

    public void UpdateFromIdP(string email, string displayName, List<string> roles, Dictionary<string, string> claims)
    {
        Email = email;
        DisplayName = displayName;
        Roles = new List<string>(roles);
        Claims = new Dictionary<string, string>(claims);
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new UserProfileUpdated(
            Id, TenantId, ExternalId, Email, DisplayName, Roles, Claims));
    }
}
