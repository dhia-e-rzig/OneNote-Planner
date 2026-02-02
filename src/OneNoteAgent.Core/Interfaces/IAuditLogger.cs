using OneNoteAgent.Core.Models;

namespace OneNoteAgent.Core.Interfaces;

/// <summary>
/// Interface for audit logging of OneNote operations.
/// Logs operation metadata only - never logs content.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an operation with success status
    /// </summary>
    Task LogOperationAsync(
        AuditOperation operation,
        string? notebookId = null,
        string? sectionId = null,
        string? pageId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an operation with failure status
    /// </summary>
    Task LogFailureAsync(
        AuditOperation operation,
        string errorMessage,
        string? notebookId = null,
        string? sectionId = null,
        string? pageId = null,
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
