namespace IdentityService.Application.Dtos;

public record UserProfileDto(
    Guid Id,
    Guid TenantId,
    string ExternalId,
    string Email,
    string DisplayName,
    List<string> Roles,
    Dictionary<string, string> Claims,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
