using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OneNoteAgent.Maui.Interfaces;
using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.ViewModels;

/// <summary>
/// ViewModel for the main chat interface.
/// Manages conversation state using GitHub Copilot SDK chat service with streaming support.
/// Slash commands are passed directly to Copilot CLI.
/// </summary>
#pragma warning disable MVVMTK0045 // WinRT AOT compatibility - acceptable for this app
public partial class ChatViewModel : ObservableObject
{
    private readonly IAuditLogger _auditLogger;
    private readonly IChatService _chatService;
    private bool _isInitialized;
    private ChatMessage? _streamingMessage;

    [ObservableProperty]
    private string _messageInput = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isThinking;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string? _currentToolName;

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public ChatViewModel(
        IAuditLogger auditLogger,
        IChatService chatService)
    {
        _auditLogger = auditLogger;
        _chatService = chatService;

        // Add welcome message
        Messages.Add(ChatMessage.System(
            "Welcome! I'm your AI assistant powered by GitHub Copilot. " +
            "Use /help to see available Copilot commands, or just ask me anything!"));

        // Initialize chat service on construction
        _ = InitializeAsync();
    }
#pragma warning restore MVVMTK0045

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = "Initializing Copilot...";

            await _chatService.InitializeAsync();
            _isInitialized = true;

            StatusMessage = "Ready";
            Messages.Add(ChatMessage.Assistant(
                "I'm ready to help! You can ask me anything or use /help to see Copilot commands."));
        }
        catch (Exception ex)
        {
            StatusMessage = "Initialization failed";
            Messages.Add(ChatMessage.Assistant($"Failed to initialize: {ex.Message}. Please restart the app."));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageInput))
            return;

        var userMessage = MessageInput.Trim();
        MessageInput = string.Empty;

        // Handle local /clear command (only one we keep locally)
        if (userMessage.Equals("/clear", StringComparison.OrdinalIgnoreCase))
        {
            Messages.Clear();
            Messages.Add(ChatMessage.System("Chat cleared. How can I help you?"));
            return;
        }

        // Show user message
        Messages.Add(ChatMessage.User(userMessage));

        if (!_isInitialized)
        {
            Messages.Add(ChatMessage.Assistant("Please wait for initialization to complete."));
            return;
        }

        // Create a streaming message placeholder
        _streamingMessage = ChatMessage.Assistant("", isStreaming: true);
        Messages.Add(_streamingMessage);

        try
        {
            IsLoading = true;
            IsThinking = true;
            StatusMessage = "Thinking...";

            // Pass everything (including slash commands) directly to Copilot CLI
            await _chatService.ProcessMessageAsync(
                userMessage, 
                Messages, 
                OnStreamingUpdate);

            // Mark streaming as complete (content was already updated via streaming)
            _streamingMessage?.CompleteStreaming();
        }
        catch (Exception ex)
        {
            // Update streaming message with error
            if (_streamingMessage is not null)
            {
                _streamingMessage.Content = $"Error: {ex.Message}";
                _streamingMessage.CompleteStreaming();
            }
        }
        finally
        {
            _streamingMessage = null;
            IsLoading = false;
            IsThinking = false;
            CurrentToolName = null;
            StatusMessage = "Ready";
        }
    }

    private void OnStreamingUpdate(StreamingUpdate update)
    {
        // Marshal to UI thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (update.Type)
            {
                case StreamingUpdateType.TextDelta:
                    if (_streamingMessage is not null && update.Content is not null)
                    {
                        // Append content directly - no collection replacement needed
                        _streamingMessage.AppendContent(update.Content);
                    }
                    IsThinking = false;
                    StatusMessage = "Responding...";
                    break;

                case StreamingUpdateType.Thinking:
                    IsThinking = true;
                    StatusMessage = "Thinking...";
                    break;

                case StreamingUpdateType.ToolStarted:
                    CurrentToolName = update.ToolName;
                    StatusMessage = $"Using tool: {update.ToolName}";
                    break;

                case StreamingUpdateType.ToolCompleted:
                    CurrentToolName = null;
                    StatusMessage = "Processing...";
                    break;

                case StreamingUpdateType.Complete:
                    IsThinking = false;
                    CurrentToolName = null;
                    StatusMessage = "Ready";
                    break;

                case StreamingUpdateType.Error:
                    IsThinking = false;
                    CurrentToolName = null;
                    StatusMessage = $"Error: {update.Content}";
                    break;
            }
        });
    }

    [RelayCommand]
    private async Task CopyMessageAsync(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return;

        await Clipboard.Default.SetTextAsync(content);
        
        // Brief visual feedback
        var originalStatus = StatusMessage;
        StatusMessage = "Copied to clipboard!";
        await Task.Delay(1500);
        StatusMessage = originalStatus;
    }

    [RelayCommand]
    private async Task ViewAuditLogAsync()
    {
        var entries = await _auditLogger.GetRecentEntriesAsync(20);
        
        if (!entries.Any())
        {
            Messages.Add(ChatMessage.System("No audit log entries yet."));
            return;
        }

        var log = string.Join("\n", entries.Select(e =>
            $"[{e.Timestamp:HH:mm:ss}] {e.Operation} - {(e.Success ? "✓" : "✗ " + e.ErrorMessage)}"));

        Messages.Add(ChatMessage.System($"**Recent Activity Log:**\n```\n{log}\n```"));
    }
}
