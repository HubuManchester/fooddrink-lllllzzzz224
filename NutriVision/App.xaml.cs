using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly IAppSettingsService _appSettingsService;

    public App(AppShell shell, IAppSettingsService appSettingsService)
    {
        InitializeComponent();
        _shell = shell;
        _appSettingsService = appSettingsService;
        _ = RestoreSettingsAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }

    private async Task RestoreSettingsAsync()
    {
        try
        {
            var settings = await _appSettingsService.LoadAsync(CancellationToken.None);
            UserAppTheme = settings.ThemeMode switch
            {
                ThemeMode.Light => AppTheme.Light,
                ThemeMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
        catch
        {
            // Keep system defaults when loading persisted settings fails.
        }
    }
}
