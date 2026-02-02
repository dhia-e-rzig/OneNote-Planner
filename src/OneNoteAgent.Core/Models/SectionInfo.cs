namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents a OneNote section within a notebook
/// </summary>
public record SectionInfo(
    string Id,
    string DisplayName,
    string? ParentNotebookId,
    DateTimeOffset? CreatedDateTime,
    DateTimeOffset? LastModifiedDateTime,
    string? SelfLink);
