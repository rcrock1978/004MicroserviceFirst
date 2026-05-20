using IdentityService.Domain;
using SaaSCommon.Domain;

namespace TestUtilities.Builders;

public class UserProfileBuilder
{
    private TenantId _tenantId = new TenantId(Guid.NewGuid());
    private string _externalId = "ext-123";
    private string _email = "user@example.com";
    private string _displayName = "Test User";
    private List<string> _roles = new() { "User" };
    private Dictionary<string, string> _claims = new();

    public UserProfileBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public UserProfileBuilder WithExternalId(string externalId)
    {
        _externalId = externalId;
        return this;
    }

    public UserProfileBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserProfileBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public UserProfileBuilder WithRoles(List<string> roles)
    {
        _roles = roles;
        return this;
    }

    public UserProfileBuilder WithClaims(Dictionary<string, string> claims)
    {
        _claims = claims;
        return this;
    }

    public UserProfile Build()
    {
        return new UserProfile(_tenantId, _externalId, _email, _displayName, _roles, _claims);
    }
}
