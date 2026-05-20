using SaaSCommon.Domain;

namespace IdentityService.Domain;

public sealed record UserProfileUpdated(
    Guid UserProfileId,
    TenantId TenantId,
    string ExternalId,
    string Email,
    string DisplayName,
    List<string> Roles,
    Dictionary<string, string> Claims
) : DomainEvent;
