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

