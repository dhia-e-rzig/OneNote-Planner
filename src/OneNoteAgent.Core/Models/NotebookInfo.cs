namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents a OneNote notebook
/// </summary>
public record NotebookInfo(
    string Id,
    string DisplayName,
    string? CreatedBy,
    DateTimeOffset? CreatedDateTime,
    DateTimeOffset? LastModifiedDateTime,
    bool IsShared,
    string? SelfLink);
