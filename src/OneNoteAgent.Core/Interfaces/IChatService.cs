using OneNoteAgent.Core.Models;

namespace OneNoteAgent.Core.Interfaces;

/// <summary>
/// Interface for LLM-based chat service that processes user messages
/// and intelligently calls MCP tools.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Processes a user message using an LLM, which may call registered tools.
    /// </summary>
    /// <param name="userMessage">The user's input message</param>
    /// <param name="conversationHistory">Previous messages in the conversation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The LLM's response after potentially calling tools</returns>
    Task<string> ProcessMessageAsync(
        string userMessage,
        IEnumerable<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the chat service with available tools.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
