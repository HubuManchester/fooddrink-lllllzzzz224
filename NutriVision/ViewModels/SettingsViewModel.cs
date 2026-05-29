using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;

namespace NutriVision.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private ThemeMode selectedTheme = ThemeMode.System;

    [ObservableProperty]
    private FontScale selectedFontScale = FontScale.Medium;

    [ObservableProperty]
    private bool ttsEnabled = true;

    [ObservableProperty]
    private bool highContrastEnabled;

    [RelayCommand]
    private Task ApplyAsync()
    {
        Application.Current!.UserAppTheme = SelectedTheme switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
        return Task.CompletedTask;
    }
}

