using CommunityToolkit.Mvvm.ComponentModel;

namespace OneNoteAgent.Maui.Models;

/// <summary>
/// Represents a message in the chat conversation.
/// Uses ObservableObject to support property change notifications for streaming updates.
/// </summary>
public partial class ChatMessage : ObservableObject
{
    public Guid Id { get; }
    public string Role { get; }
    public DateTimeOffset Timestamp { get; }

    [ObservableProperty]
    public partial string Content { get; set; }

    [ObservableProperty]
    public partial bool IsStreaming { get; set; }

    public ChatMessage(Guid id, string role, string content, DateTimeOffset timestamp, bool isStreaming = false)
    {
        Id = id;
        Role = role;
        Content = content;
        Timestamp = timestamp;
        IsStreaming = isStreaming;
    }

    public static ChatMessage User(string content) =>
        new(Guid.NewGuid(), "user", content, DateTimeOffset.UtcNow);

    public static ChatMessage Assistant(string content, bool isStreaming = false) =>
        new(Guid.NewGuid(), "assistant", content, DateTimeOffset.UtcNow, isStreaming);

    public static ChatMessage System(string content) =>
        new(Guid.NewGuid(), "system", content, DateTimeOffset.UtcNow);

    /// <summary>
    /// Appends content to the message (for streaming).
    /// </summary>
    public void AppendContent(string text)
    {
        Content += text;
    }

    /// <summary>
    /// Marks the message as complete (no longer streaming).
    /// </summary>
    public void CompleteStreaming()
    {
        IsStreaming = false;
    }
}
