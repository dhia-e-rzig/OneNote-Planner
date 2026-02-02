using System.Globalization;

namespace OneNoteAgent.Maui.Converters;

/// <summary>
/// Converts chat message role to a display-friendly name
/// </summary>
public class RoleToDisplayNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToLowerInvariant() switch
        {
            "user" => "You",
            "assistant" => "Copilot",
            "system" => "System",
            _ => value?.ToString()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts chat message role to background color
/// </summary>
public class RoleToBackgroundColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var role = value?.ToString()?.ToLowerInvariant();
        
        return role switch
        {
            "user" => Application.Current?.RequestedTheme == AppTheme.Dark 
                ? Color.FromArgb("#1E3A5F") 
                : Color.FromArgb("#E3F2FD"),
            "assistant" => Application.Current?.RequestedTheme == AppTheme.Dark 
                ? Color.FromArgb("#2D2D2D") 
                : Color.FromArgb("#F5F5F5"),
            "system" => Application.Current?.RequestedTheme == AppTheme.Dark 
                ? Color.FromArgb("#3D3D1D") 
                : Color.FromArgb("#FFF8E1"),
            _ => Colors.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Inverts a boolean value
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}

/// <summary>
/// Returns true if the value is not null
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
