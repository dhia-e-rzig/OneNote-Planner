namespace OneNoteAgent.Maui.Interfaces;

/// <summary>
/// Provides navigation services for the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    Task NavigateToAsync<TPage>() where TPage : Page;

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    Task GoBackAsync();
}
