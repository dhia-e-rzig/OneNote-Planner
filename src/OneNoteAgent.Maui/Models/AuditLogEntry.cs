namespace OneNoteAgent.Maui.Models;

/// <summary>
/// Represents an audit log entry for operations.
/// Contains operation metadata only.
/// </summary>
public record AuditLogEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    AuditOperation Operation,
    bool Success,
    string? ErrorMessage);

/// <summary>
/// Types of auditable operations
/// </summary>
public enum AuditOperation
{
    ChatMessage,
    Initialize,
    Error
}
