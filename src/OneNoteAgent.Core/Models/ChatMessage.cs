namespace OneNoteAgent.Core.Models;

/// <summary>
/// Represents a message in the chat conversation
/// </summary>
public record ChatMessage(
    Guid Id,
    string Role,
    string Content,
    DateTimeOffset Timestamp,
    bool IsStreaming = false)
{
    public static ChatMessage User(string content) =>
        new(Guid.NewGuid(), "user", content, DateTimeOffset.UtcNow);

    public static ChatMessage Assistant(string content, bool isStreaming = false) =>
        new(Guid.NewGuid(), "assistant", content, DateTimeOffset.UtcNow, isStreaming);

    public static ChatMessage System(string content) =>
        new(Guid.NewGuid(), "system", content, DateTimeOffset.UtcNow);
}
