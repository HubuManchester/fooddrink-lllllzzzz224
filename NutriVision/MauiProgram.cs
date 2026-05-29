using Microsoft.Extensions.Logging;
using NutriVision.Services;
using NutriVision.Services.Abstractions;
using NutriVision.ViewModels;
using NutriVision.Views;

namespace NutriVision;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Backend services
        builder.Services.AddSingleton<IHistoryRepository, HistoryRepository>();
        builder.Services.AddSingleton<ILocalFallbackRecognitionService, LocalFallbackRecognitionService>();
        builder.Services.AddSingleton<INutritionService, NutritionService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<ISpeechService, SpeechService>();
        builder.Services.AddSingleton<IHapticService, HapticService>();
        builder.Services.AddSingleton<IShakeService, ShakeService>();
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<IScanWorkflowService, ScanWorkflowService>();
        builder.Services.AddHttpClient<IFoodRecognitionService, FoodRecognitionService>(client =>
        {
            client.BaseAddress = new Uri("https://example.com");
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        // Frontend view models
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Frontend pages
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<ResultPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<SettingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

