using OneNoteAgent.Maui.Views;

namespace OneNoteAgent.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(ActivityPage), typeof(ActivityPage));
    }
}
