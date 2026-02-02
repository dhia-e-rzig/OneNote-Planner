namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents a OneNote page within a section
/// </summary>
public record PageInfo(
    string Id,
    string Title,
    string? ParentSectionId,
    DateTimeOffset? CreatedDateTime,
    DateTimeOffset? LastModifiedDateTime,
    int Level,
    int Order,
    string? ContentUrl);
