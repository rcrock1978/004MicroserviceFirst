namespace SaaSCommon.Domain;

public record Error(string Code, string Message, string? Details = null)
{
    public static readonly Error NotFound = new("Error.NotFound", "The requested resource was not found.");
    public static readonly Error Validation = new("Error.Validation", "A validation error occurred.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "You are not authorized to perform this action.");
    public static readonly Error Conflict = new("Error.Conflict", "A conflict occurred with the current state of the resource.");

    public static Error NotFoundWithDetails(string details) => NotFound with { Details = details };
    public static Error ValidationWithDetails(string details) => Validation with { Details = details };
    public static Error UnauthorizedWithDetails(string details) => Unauthorized with { Details = details };
    public static Error ConflictWithDetails(string details) => Conflict with { Details = details };
}
