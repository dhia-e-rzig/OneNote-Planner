namespace OneNoteAgent.Maui.Views;

public partial class ActivityPage : ContentPage
{
    public ActivityPage(ViewModels.ActivityViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
