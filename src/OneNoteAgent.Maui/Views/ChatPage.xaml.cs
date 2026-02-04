using System.Collections.Specialized;
using OneNoteAgent.Maui.ViewModels;

namespace OneNoteAgent.Maui.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Auto-scroll to bottom when new messages are added
        viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;

        // Auto-scroll thinking content when it updates
        viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Set up keyboard handling for the editor
        MessageEditor.HandlerChanged += OnEditorHandlerChanged;
    }

    private void OnEditorHandlerChanged(object? sender, EventArgs e)
    {
#if WINDOWS
        if (MessageEditor.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.TextBox textBox)
        {
            textBox.AcceptsReturn = true; // Allow multi-line by default
            textBox.PreviewKeyDown += OnTextBoxPreviewKeyDown;
        }
#endif
    }

#if WINDOWS
    private void OnTextBoxPreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
            var altState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Menu);
            var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            
            bool isShiftPressed = shiftState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool isAltPressed = altState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            bool isCtrlPressed = ctrlState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (!isShiftPressed && !isAltPressed && !isCtrlPressed)
            {
                // Plain Enter - send the message
                e.Handled = true;
                
                Dispatcher.Dispatch(() =>
                {
                    if (_viewModel.SendMessageCommand.CanExecute(null))
                    {
                        _viewModel.SendMessageCommand.Execute(null);
                    }
                });
            }
            // Shift+Enter, Alt+Enter, or Ctrl+Enter - allow default behavior (new line)
        }
    }
#endif

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChatViewModel.ThinkingContent))
        {
            ScrollThinkingToBottom();
        }
    }

    private void ScrollThinkingToBottom()
    {
        Dispatcher.Dispatch(async () =>
        {
            // Small delay to allow layout to update
            await Task.Delay(50);
            await ThinkingScrollView.ScrollToAsync(0, ThinkingScrollView.ContentSize.Height, animated: false);
        });
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Only scroll on Add - property changes don't trigger CollectionChanged
        if (e.Action != NotifyCollectionChangedAction.Add)
            return;

        // Use Dispatcher to ensure we scroll after the UI has updated
        Dispatcher.Dispatch(async () =>
        {
            // Wait for layout to complete
            await Task.Delay(100);
            
            var lastItem = _viewModel.Messages.LastOrDefault();
            if (lastItem is not null)
            {
                try
                {
                    MessagesCollectionView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: false);
                }
                catch
                {
                    // Ignore scroll errors - can happen if collection changed during scroll
                }
            }
        });
    }
}

