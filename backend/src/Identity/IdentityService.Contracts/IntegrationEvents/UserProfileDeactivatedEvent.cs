namespace IdentityService.Contracts.IntegrationEvents;

public record UserProfileDeactivatedEvent(
    Guid UserProfileId,
    Guid TenantId,
    string ExternalId,
    DateTime OccurredOn
);
