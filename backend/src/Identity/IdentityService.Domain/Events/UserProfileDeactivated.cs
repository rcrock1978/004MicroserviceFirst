using SaaSCommon.Domain;

namespace IdentityService.Domain;

public sealed record UserProfileDeactivated(
    Guid UserProfileId,
    TenantId TenantId,
    string ExternalId
) : DomainEvent;
