namespace TenantService.Domain;

public sealed record FeatureFlag(string Key, bool Enabled, string? Description = null);
