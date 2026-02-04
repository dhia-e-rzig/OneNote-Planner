namespace OneNoteAgent.Maui.Models;

/// <summary>
/// Represents an audit log entry for operations.
/// Contains operation metadata including tool inputs and outputs.
/// </summary>
public record AuditLogEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    AuditOperation Operation,
    bool Success,
    string? ErrorMessage,
    string? ToolName = null,
    string? ToolInput = null,
    string? ToolOutput = null);

/// <summary>
/// Types of auditable operations
/// </summary>
public enum AuditOperation
{
    ToolCall,
    Error
}
