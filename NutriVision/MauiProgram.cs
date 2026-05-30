using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var cloudApiUrl = Environment.GetEnvironmentVariable("NUTRIVISION_FOOD_API_URL");
        var cloudApiToken = Environment.GetEnvironmentVariable("NUTRIVISION_FOOD_API_TOKEN");
        builder.Services.AddSingleton(new FoodRecognitionOptions
        {
            ApiUrl = string.IsNullOrWhiteSpace(cloudApiUrl)
                ? "https://api-inference.huggingface.co/models/nateraw/food"
                : cloudApiUrl,
            ApiToken = cloudApiToken
        });

        // Backend services
        builder.Services.AddSingleton<IHistoryRepository, HistoryRepository>();
        builder.Services.AddSingleton<ILocalFallbackRecognitionService, LocalFallbackRecognitionService>();
        builder.Services.AddSingleton<INutritionService, NutritionService>();
        builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<ISpeechService, SpeechService>();
        builder.Services.AddSingleton<IHapticService, HapticService>();
        builder.Services.AddSingleton<IShakeService, ShakeService>();
        builder.Services.AddSingleton<IMicrophoneService, MicrophoneService>();
        builder.Services.AddSingleton<ICameraService, CameraService>();
        builder.Services.AddSingleton<IScanWorkflowService, ScanWorkflowService>();
        builder.Services.AddHttpClient<IFoodRecognitionService, FoodRecognitionService>(client => client.Timeout = TimeSpan.FromSeconds(8));

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
