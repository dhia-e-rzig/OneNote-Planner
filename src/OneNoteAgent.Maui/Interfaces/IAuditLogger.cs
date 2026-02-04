using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.Interfaces;

/// <summary>
/// Interface for audit logging of operations.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an operation with success status
    /// </summary>
    Task LogOperationAsync(
        AuditOperation operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a tool operation with the tool name, input, and output
    /// </summary>
    Task LogToolOperationAsync(
        AuditOperation operation,
        string toolName,
        string? toolInput = null,
        string? toolOutput = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an operation with failure status
    /// </summary>
    Task LogFailureAsync(
        AuditOperation operation,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent audit log entries for user review
    /// </summary>
    Task<IReadOnlyList<AuditLogEntry>> GetRecentEntriesAsync(
        int count = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all audit log entries
    /// </summary>
    Task ClearLogsAsync(CancellationToken cancellationToken = default);
}
