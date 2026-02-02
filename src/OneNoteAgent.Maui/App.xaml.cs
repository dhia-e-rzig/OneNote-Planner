using OneNoteAgent.Maui.Views;

namespace OneNoteAgent.Maui;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var chatPage = _serviceProvider.GetRequiredService<ChatPage>();
        return new Window(new NavigationPage(chatPage))
        {
            Title = "OneNote Agent"
        };
    }
}
