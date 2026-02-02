namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents an audit log entry for OneNote operations.
/// Contains operation metadata only - never stores content.
/// </summary>
public record AuditLogEntry(
    Guid Id,
    DateTimeOffset Timestamp,
    AuditOperation Operation,
    string? NotebookId,
    string? SectionId,
    string? PageId,
    bool Success,
    string? ErrorMessage);

/// <summary>
/// Types of auditable operations
/// </summary>
public enum AuditOperation
{
    ListNotebooks,
    GetNotebook,
    ListSections,
    ListPages,
    GetPageContent,
    CreatePage,
    UpdatePage,
    DeletePage,
    SearchNotes,
    Authenticate
}
