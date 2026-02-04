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
/// Converts chat message role to a background brush for message styling.
/// Looks up theme-specific colors from Colors.xaml resources.
/// </summary>
public class RoleToBackgroundColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var role = value?.ToString()?.ToLowerInvariant();
        var isDarkTheme = IsDarkTheme();
        
        System.Diagnostics.Debug.WriteLine($"[RoleToBackgroundColorConverter] Role='{role}', IsDark={isDarkTheme}");
        
        var baseResourceKey = role switch
        {
            "user" => "UserMessageBackground",
            "assistant" => "AssistantMessageBackground",
            "system" => "SystemMessageBackground",
            _ => null
        };

        Color color;
        if (baseResourceKey is null)
        {
            color = GetFallbackColor(role, isDarkTheme);
            System.Diagnostics.Debug.WriteLine($"[RoleToBackgroundColorConverter] No base key, returning fallback: {color}");
        }
        else
        {
            // Determine theme suffix based on current app theme
            var themeSuffix = isDarkTheme ? "Dark" : "Light";
            var resourceKey = $"{baseResourceKey}{themeSuffix}";

            // Look up the theme-specific color from application resources (including merged dictionaries)
            if (TryGetResource(resourceKey, out var resourceColor) && resourceColor is Color colorValue)
            {
                System.Diagnostics.Debug.WriteLine($"[RoleToBackgroundColorConverter] Found resource '{resourceKey}': {colorValue}");
                color = colorValue;
            }
            else
            {
                // Fallback colors if resources aren't defined
                color = GetFallbackColor(role, isDarkTheme);
                System.Diagnostics.Debug.WriteLine($"[RoleToBackgroundColorConverter] Resource '{resourceKey}' not found, returning fallback: {color}");
            }
        }

        // Return a SolidColorBrush for use with Border.Background property
        return new SolidColorBrush(color);
    }

    private static Color GetFallbackColor(string? role, bool isDarkTheme)
    {
        return (role, isDarkTheme) switch
        {
            ("user", false) => Color.FromArgb("#E3F2FD"),
            ("user", true) => Color.FromArgb("#1E3A5F"),
            ("assistant", false) => Color.FromArgb("#FFFFFF"),
            ("assistant", true) => Color.FromArgb("#2D2D2D"),
            ("system", false) => Color.FromArgb("#FFF8E1"),
            ("system", true) => Color.FromArgb("#3D3D1D"),
            _ => Colors.Transparent
        };
    }

    private static bool IsDarkTheme()
    {
        var app = Application.Current;
        if (app is null)
            return false;

        var theme = app.PlatformAppTheme;
        if (app.UserAppTheme != AppTheme.Unspecified)
        {
            theme = app.UserAppTheme;
        }
        return theme == AppTheme.Dark;
    }

    private static bool TryGetResource(string key, out object? value)
    {
        value = null;
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            System.Diagnostics.Debug.WriteLine($"[RoleToBackgroundColorConverter] Application.Current?.Resources is null!");
            return false;
        }

        // First try direct lookup
        if (resources.TryGetValue(key, out value))
            return true;

        // Search merged dictionaries
        foreach (var merged in resources.MergedDictionaries)
        {
            if (merged.TryGetValue(key, out value))
                return true;
        }

        return false;
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

/// <summary>
/// Returns true if the value is null
/// </summary>
public class IsNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts markdown text to HTML for display in a Label with TextType="Html".
/// Uses Markdig for markdown parsing and applies theme-aware styling.
/// </summary>
public partial class MarkdownToHtmlConverter : IValueConverter
{
    private static readonly Markdig.MarkdownPipeline Pipeline = Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();

    // Regex patterns for cleaning up HTML
    [System.Text.RegularExpressions.GeneratedRegex(@"<p>\s*", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex OpenParagraphRegex();
    
    [System.Text.RegularExpressions.GeneratedRegex(@"\s*</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex CloseParagraphRegex();
    
    [System.Text.RegularExpressions.GeneratedRegex(@"(<br\s*/?>)+\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex TrailingBreaksRegex();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string markdown || string.IsNullOrWhiteSpace(markdown))
            return value;

        var html = Markdig.Markdown.ToHtml(markdown, Pipeline);
        
        // Replace <p> tags with line breaks to avoid paragraph margins
        // Remove opening <p> tags
        html = OpenParagraphRegex().Replace(html, "");
        // Replace closing </p> tags with double line breaks (except for the last one)
        html = CloseParagraphRegex().Replace(html, "<br><br>");
        
        // Trim and remove trailing breaks
        html = html.Trim();
        html = TrailingBreaksRegex().Replace(html, "");
        
        // Wrap in a span with theme-aware text color
        // Label.TextColor doesn't apply when TextType="Html", so we must style inline
        var textColor = GetCurrentTextColor();
        html = $"<span style=\"color:{textColor}\">{html}</span>";
        
        return html;
    }

    private static string GetCurrentTextColor()
    {
        var app = Application.Current;
        if (app is null)
            return "#000000";

        var theme = app.PlatformAppTheme;
        if (app.UserAppTheme != AppTheme.Unspecified)
        {
            theme = app.UserAppTheme;
        }
        
        // Return white for dark theme, black for light theme
        return theme == AppTheme.Dark ? "#FFFFFF" : "#000000";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts a boolean success value to a color (green for success, red for failure).
/// </summary>
public class BoolToSuccessColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true 
            ? Color.FromArgb("#28A745")  // Green for success
            : Color.FromArgb("#DC3545"); // Red for failure
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
