using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.Interfaces;

/// <summary>
/// Represents streaming progress from the chat service.
/// </summary>
public record StreamingUpdate(
    StreamingUpdateType Type,
    string? Content = null,
    string? ToolName = null,
    string? ToolInput = null,
    string? ToolResult = null);

/// <summary>
/// Types of streaming updates.
/// </summary>
public enum StreamingUpdateType
{
    /// <summary>Incremental text content from the assistant.</summary>
    TextDelta,
    /// <summary>A tool has started executing.</summary>
    ToolStarted,
    /// <summary>A tool has finished executing.</summary>
    ToolCompleted,
    /// <summary>The assistant is thinking/processing.</summary>
    Thinking,
    /// <summary>The response is complete.</summary>
    Complete,
    /// <summary>An error occurred.</summary>
    Error
}

/// <summary>
/// Interface for chat service that processes user messages using GitHub Copilot SDK.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Processes a user message using GitHub Copilot with streaming support.
    /// </summary>
    /// <param name="userMessage">The user's input message</param>
    /// <param name="conversationHistory">Previous messages in the conversation</param>
    /// <param name="onUpdate">Callback for streaming updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The complete assistant's response</returns>
    Task<string> ProcessMessageAsync(
        string userMessage,
        IEnumerable<ChatMessage> conversationHistory,
        Action<StreamingUpdate>? onUpdate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the chat service.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
