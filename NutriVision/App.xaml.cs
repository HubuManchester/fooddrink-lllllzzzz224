using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAppAppearanceService _appAppearanceService;

    public App(AppShell shell, IAppSettingsService appSettingsService, IAppAppearanceService appAppearanceService)
    {
        InitializeComponent();
        _shell = shell;
        _appSettingsService = appSettingsService;
        _appAppearanceService = appAppearanceService;
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
            _appAppearanceService.Apply(settings);
        }
        catch
        {
            // Keep system defaults when loading persisted settings fails.
        }
    }
}
