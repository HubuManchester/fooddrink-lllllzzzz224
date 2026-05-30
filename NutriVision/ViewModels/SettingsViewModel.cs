using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAppSettingsService _appSettingsService;

    [ObservableProperty]
    private int selectedThemeIndex = (int)ThemeMode.System;

    [ObservableProperty]
    private ThemeMode selectedTheme = ThemeMode.System;

    [ObservableProperty]
    private int selectedFontScaleIndex = (int)FontScale.Medium;

    [ObservableProperty]
    private FontScale selectedFontScale = FontScale.Medium;

    [ObservableProperty]
    private bool ttsEnabled = true;

    [ObservableProperty]
    private bool highContrastEnabled;

    [ObservableProperty]
    private string applyMessage = string.Empty;

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
            ApplyMessage = "Using default settings.";
        }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        try
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

            await _appSettingsService.SaveAsync(settings, CancellationToken.None);
            ApplyMessage = "Settings saved.";
        }
        catch
        {
            ApplyMessage = "Failed to save settings. Please retry.";
        }
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        if (value is < 0 or > 2)
        {
            return;
        }

        SelectedTheme = (ThemeMode)value;
    }

    partial void OnSelectedThemeChanged(ThemeMode value)
    {
        var idx = (int)value;
        if (SelectedThemeIndex != idx)
        {
            SelectedThemeIndex = idx;
        }
    }

    partial void OnSelectedFontScaleIndexChanged(int value)
    {
        if (value is < 0 or > 2)
        {
            return;
        }

        SelectedFontScale = (FontScale)value;
    }

    partial void OnSelectedFontScaleChanged(FontScale value)
    {
        var idx = (int)value;
        if (SelectedFontScaleIndex != idx)
        {
            SelectedFontScaleIndex = idx;
        }
    }
}
