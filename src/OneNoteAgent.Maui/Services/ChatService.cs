using GitHub.Copilot.SDK;
using OneNoteAgent.Maui.Interfaces;
using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.Services;

/// <summary>
/// Chat service implementation using GitHub Copilot SDK.
/// Configures MCP servers for extended AI capabilities.
/// </summary>
public sealed class ChatService : IChatService, IAsyncDisposable
{
    private CopilotClient? _copilotClient;
    private CopilotSession? _session;
    private bool _isInitialized;

    private const string SystemInstructions = """
        You are a helpful AI assistant powered by GitHub Copilot.
        You have access to various tools including filesystem operations, 
        Microsoft Learn documentation, and GitHub integration.
        
        Be concise and helpful in your responses.
        """;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        // Get user's home directory for filesystem access
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        _copilotClient = new CopilotClient(new CopilotClientOptions
        {
            Cwd = homeDir,
            UseLoggedInUser = true
        });
        await _copilotClient.StartAsync();

        // Create a session with MCP servers configured
        _session = await _copilotClient.CreateSessionAsync(new SessionConfig
        {
            Model = "claude-sonnet-4",
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = SystemInstructions
            },
            Streaming = true,
            // Configure MCP servers for extended capabilities
            McpServers = new Dictionary<string, object>
            {
                //// Local filesystem server - provides file read/write/search tools
                //["filesystem"] = new McpLocalServerConfig
                //{
                //    Type = "stdio",
                //    Command = "npx",
                //    Args = ["-y", "@modelcontextprotocol/server-filesystem", homeDir],
                //    Tools = ["*"]
                //},
                //// Microsoft Learn documentation server
                //["microsoft-learn"] = new McpRemoteServerConfig
                //{
                //    Type = "http",
                //    Url = "https://learn.microsoft.com/api/mcp",
                //    Tools = ["*"]
                //},
                //// GitHub MCP server - provides GitHub tools (requires auth)
                //["github"] = new McpRemoteServerConfig
                //{
                //    Type = "http",
                //    Url = "https://api.githubcopilot.com/mcp/",
                //    Tools = ["*"]
                //},
                // Microsoft WorkIQ server - provides productivity tools
                ["workiq"] = new McpLocalServerConfig
                {
                    Type = "stdio",
                    Command = "npx",
                    Args = ["-y", "@microsoft/workiq", "mcp"],
                    Tools = ["*"]
                }
            }
        });

        _isInitialized = true;
    }

    /// <inheritdoc />
    public async Task<string> ProcessMessageAsync(
        string userMessage,
        IEnumerable<ChatMessage> conversationHistory,
        Action<StreamingUpdate>? onUpdate = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _session is null)
        {
            throw new InvalidOperationException("ChatService is not initialized. Call InitializeAsync first.");
        }

        try
        {
            var responseBuilder = new System.Text.StringBuilder();
            var completionSource = new TaskCompletionSource<string>();

            // Notify that we're thinking
            onUpdate?.Invoke(new StreamingUpdate(StreamingUpdateType.Thinking));

            // Subscribe to session events
            using var subscription = _session.On(evt =>
            {
                switch (evt)
                {
                    case AssistantMessageDeltaEvent delta:
                        // Streaming chunk - append incrementally
                        responseBuilder.Append(delta.Data.DeltaContent);
                        onUpdate?.Invoke(new StreamingUpdate(
                            StreamingUpdateType.TextDelta, 
                            Content: delta.Data.DeltaContent));
                        break;
                    
                    case AssistantMessageEvent msg:
                        // Final message - use complete content if we haven't accumulated anything
                        if (responseBuilder.Length == 0)
                        {
                            responseBuilder.Append(msg.Data.Content);
                        }
                        break;
                    
                    case AssistantReasoningDeltaEvent reasoningDelta:
                        // Model is reasoning/thinking
                        onUpdate?.Invoke(new StreamingUpdate(
                            StreamingUpdateType.Thinking,
                            Content: reasoningDelta.Data.DeltaContent));
                        break;
                    
                    case ToolExecutionStartEvent toolStart:
                        onUpdate?.Invoke(new StreamingUpdate(
                            StreamingUpdateType.ToolStarted,
                            ToolName: toolStart.Data.ToolName));
                        break;
                    
                    case ToolExecutionCompleteEvent:
                        onUpdate?.Invoke(new StreamingUpdate(StreamingUpdateType.ToolCompleted));
                        break;
                    
                    case SessionIdleEvent:
                        onUpdate?.Invoke(new StreamingUpdate(StreamingUpdateType.Complete));
                        completionSource.TrySetResult(responseBuilder.ToString());
                        break;
                    
                    case SessionErrorEvent err:
                        onUpdate?.Invoke(new StreamingUpdate(
                            StreamingUpdateType.Error,
                            Content: err.Data.Message));
                        completionSource.TrySetException(new Exception(err.Data.Message));
                        break;
                }
            });

            // Send the message
            await _session.SendAsync(new MessageOptions { Prompt = userMessage });

            // Wait for completion with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(5));

            var completedTask = await Task.WhenAny(
                completionSource.Task,
                Task.Delay(Timeout.Infinite, cts.Token));

            if (completedTask != completionSource.Task)
            {
                return "Request timed out. Please try again.";
            }

            return await completionSource.Task;
        }
        catch (OperationCanceledException)
        {
            return "Request was cancelled.";
        }
        catch (Exception ex)
        {
            return $"I encountered an error processing your request: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_session is not null)
        {
            await _session.DisposeAsync();
            _session = null;
        }

        if (_copilotClient is not null)
        {
            await _copilotClient.DisposeAsync();
            _copilotClient = null;
        }

        _isInitialized = false;
    }
}
