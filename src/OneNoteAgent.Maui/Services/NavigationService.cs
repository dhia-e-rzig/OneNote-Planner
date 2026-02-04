using OneNoteAgent.Maui.Interfaces;

namespace OneNoteAgent.Maui.Services;

/// <summary>
/// Navigation service implementation using NavigationPage.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task NavigateToAsync<TPage>() where TPage : Page
    {
        var page = _serviceProvider.GetRequiredService<TPage>();
        var navigation = GetNavigation();
        if (navigation is not null)
        {
            await navigation.PushAsync(page);
        }
    }

    public async Task GoBackAsync()
    {
        var navigation = GetNavigation();
        if (navigation is not null)
        {
            await navigation.PopAsync();
        }
    }

    private static INavigation? GetNavigation()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        return (window?.Page as NavigationPage)?.Navigation;
    }
}
