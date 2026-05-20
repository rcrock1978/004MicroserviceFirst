namespace IdentityService.Contracts.IntegrationEvents;

public record UserProfileUpdatedEvent(
    Guid UserProfileId,
    Guid TenantId,
    string ExternalId,
    string Email,
    string DisplayName,
    List<string> Roles,
    Dictionary<string, string> Claims,
    DateTime OccurredOn
);
