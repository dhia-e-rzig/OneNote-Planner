namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents the content of a OneNote page
/// </summary>
public record PageContent(
    string Id,
    string Title,
    string HtmlContent,
    DateTimeOffset? LastModifiedDateTime);
