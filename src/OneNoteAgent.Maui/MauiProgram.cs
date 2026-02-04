using CommunityToolkit.Maui;
using OneNoteAgent.Maui.Interfaces;
using OneNoteAgent.Maui.Services;
using OneNoteAgent.Maui.ViewModels;
using OneNoteAgent.Maui.Views;
#if WINDOWS
using OneNoteAgent.Maui.Handlers;
#endif

namespace OneNoteAgent.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            Console.WriteLine("Starting MauiApp creation...");
            var builder = MauiApp.CreateBuilder();
            Console.WriteLine("Builder created, configuring...");
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if WINDOWS
        // Enable text selection on Labels for Windows
        SelectableLabelHandler.Configure();
#endif

        // Register services
        builder.Services.AddSingleton<IAuditLogger, AuditLogger>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        
        // Chat service with GitHub Copilot SDK integration
        builder.Services.AddSingleton<IChatService, ChatService>();

        // ViewModels
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<ActivityViewModel>();

        // Pages
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<ActivityPage>();

            Console.WriteLine("Services registered, building app...");
            var app = builder.Build();
            Console.WriteLine("MauiApp built successfully!");
            return app;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex}");
            throw;
        }
    }
}
