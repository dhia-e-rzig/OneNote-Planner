using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OneNoteAgent.Maui.Interfaces;
using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.ViewModels;

/// <summary>
/// ViewModel for the Activity page showing recent operations.
/// </summary>
public partial class ActivityViewModel : ObservableObject
{
    private readonly IAuditLogger _auditLogger;
    private readonly INavigationService _navigationService;

    public ObservableCollection<AuditLogEntry> Activities { get; } = [];

    public ActivityViewModel(IAuditLogger auditLogger, INavigationService navigationService)
    {
        _auditLogger = auditLogger;
        _navigationService = navigationService;
        _ = LoadActivitiesAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadActivitiesAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private async Task LoadActivitiesAsync()
    {
        var entries = await _auditLogger.GetRecentEntriesAsync(50);
        
        Activities.Clear();
        foreach (var entry in entries)
        {
            Activities.Add(entry);
        }
    }
}
