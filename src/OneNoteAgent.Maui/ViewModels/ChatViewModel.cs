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
public partial class ChatViewModel : ObservableObject
{
private readonly IAuditLogger _auditLogger;
private readonly IChatService _chatService;
private readonly INavigationService _navigationService;
private bool _isInitialized;
private ChatMessage? _streamingMessage;
private CancellationTokenSource? _currentOperationCts;

[ObservableProperty]
public partial string MessageInput { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsThinking { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string? CurrentToolName { get; set; }

    [ObservableProperty]
    public partial string? ThinkingContent { get; set; }

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public ChatViewModel(
        IAuditLogger auditLogger,
        IChatService chatService,
        INavigationService navigationService)
    {
        _auditLogger = auditLogger;
        _chatService = chatService;
        _navigationService = navigationService;

        MessageInput = string.Empty;
        StatusMessage = "Ready";

        // Add welcome message
        Messages.Add(ChatMessage.System(
            "Welcome! I'm your AI assistant powered by GitHub Copilot. " +
            "Use /help to see available Copilot commands, or just ask me anything!"));

        // Initialize chat service on construction
        _ = InitializeAsync();
    }


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
    private void ResetConversation()
    {
        Messages.Clear();
        Messages.Add(ChatMessage.System("Conversation reset. How can I help you?"));
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

        // Create cancellation token for this operation
        _currentOperationCts?.Dispose();
        _currentOperationCts = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            IsThinking = true;
            StatusMessage = "Thinking...";

            // Pass everything (including slash commands) directly to Copilot CLI
            await _chatService.ProcessMessageAsync(
                userMessage, 
                Messages, 
                OnStreamingUpdate,
                _currentOperationCts.Token);

            // Mark streaming as complete (content was already updated via streaming)
            _streamingMessage?.CompleteStreaming();
        }
        catch (OperationCanceledException)
        {
            // User cancelled the operation
            if (_streamingMessage is not null)
            {
                if (string.IsNullOrEmpty(_streamingMessage.Content))
                {
                    _streamingMessage.Content = "*Stopped*";
                }
                else
                {
                    _streamingMessage.AppendContent("\n\n*Stopped*");
                }
                _streamingMessage.CompleteStreaming();
            }
            StatusMessage = "Stopped";
        }
        catch (Exception ex)
        {
            // Update streaming message with error
            if (_streamingMessage is not null)
            {
                _streamingMessage.Content = $"Error: {ex.Message}";
                _streamingMessage.CompleteStreaming();
            }
            await _auditLogger.LogFailureAsync(AuditOperation.Error, ex.Message);
        }
        finally
        {
            _streamingMessage = null;
            _currentOperationCts?.Dispose();
            _currentOperationCts = null;
            IsLoading = false;
            IsThinking = false;
            CurrentToolName = null;
            ThinkingContent = null;
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    private void Stop()
    {
        _currentOperationCts?.Cancel();
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
                    ThinkingContent = null;
                    StatusMessage = "Responding...";
                    break;

                case StreamingUpdateType.Thinking:
                    IsThinking = true;
                    StatusMessage = "Thinking...";
                    if (!string.IsNullOrEmpty(update.Content))
                    {
                        // Append thinking deltas rather than replacing
                        ThinkingContent = (ThinkingContent ?? "") + update.Content;
                    }
                    break;

                case StreamingUpdateType.ToolStarted:
                    // Clear thinking content when starting a new tool
                    ThinkingContent = null;
                    CurrentToolName = update.ToolName;
                    StatusMessage = $"Using tool: {update.ToolName}";
                    break;

                case StreamingUpdateType.ToolCompleted:
                    if (!string.IsNullOrEmpty(update.ToolName))
                    {
                        _ = _auditLogger.LogToolOperationAsync(
                            AuditOperation.ToolCall, 
                            update.ToolName, 
                            update.ToolInput, 
                            update.ToolResult);
                    }
                    CurrentToolName = null;
                    // Clear thinking content - fresh thinking will come via subsequent Thinking updates
                    ThinkingContent = null;
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
    private async Task ViewActivityAsync()
    {
        await _navigationService.NavigateToAsync<Views.ActivityPage>();
    }
}
