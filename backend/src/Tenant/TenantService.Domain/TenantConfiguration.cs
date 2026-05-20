namespace TenantService.Domain;

public sealed record TenantConfiguration
{
    public string TimeZone { get; init; } = "UTC";
    public string DefaultLanguage { get; init; } = "en";
    public int MaxUsers { get; init; } = 100;
    public Dictionary<string, string> Settings { get; init; } = new();
}
