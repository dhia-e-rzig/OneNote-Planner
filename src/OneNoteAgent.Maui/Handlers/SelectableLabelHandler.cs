#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace OneNoteAgent.Maui.Handlers;

/// <summary>
/// Custom handler to make Label text selectable on Windows.
/// </summary>
public static class SelectableLabelHandler
{
    public static void Configure()
    {
        LabelHandler.Mapper.AppendToMapping("SelectableLabel", (handler, view) =>
        {
            if (handler.PlatformView is TextBlock textBlock)
            {
                textBlock.IsTextSelectionEnabled = true;
            }
        });
    }
}
#endif
