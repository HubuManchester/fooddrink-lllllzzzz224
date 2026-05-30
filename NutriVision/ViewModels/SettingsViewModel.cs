using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAppSettingsService _appSettingsService;

    [ObservableProperty]
    private ThemeMode selectedTheme = ThemeMode.System;

    [ObservableProperty]
    private FontScale selectedFontScale = FontScale.Medium;

    [ObservableProperty]
    private bool ttsEnabled = true;

    [ObservableProperty]
    private bool highContrastEnabled;

    public SettingsViewModel(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var settings = await _appSettingsService.LoadAsync(CancellationToken.None);
            SelectedTheme = settings.ThemeMode;
            SelectedFontScale = settings.FontScale;
            TtsEnabled = settings.TtsEnabled;
            HighContrastEnabled = settings.HighContrastEnabled;
        }
        catch
        {
            // Keep defaults when settings cannot be loaded.
        }
    }

    [RelayCommand]
    private Task ApplyAsync()
    {
        Application.Current!.UserAppTheme = SelectedTheme switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        var settings = new AppSettings
        {
            ThemeMode = SelectedTheme,
            FontScale = SelectedFontScale,
            TtsEnabled = TtsEnabled,
            HighContrastEnabled = HighContrastEnabled
        };

        return _appSettingsService.SaveAsync(settings, CancellationToken.None);
    }
}
