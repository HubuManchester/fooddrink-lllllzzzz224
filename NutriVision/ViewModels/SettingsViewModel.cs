using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAppAppearanceService _appAppearanceService;
    private readonly ICompassService _compassService;
    private readonly IAccelerometerService _accelerometerService;

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

    [ObservableProperty]
    private bool isCompassSupported;

    [ObservableProperty]
    private bool isCompassMonitoring;

    [ObservableProperty]
    private double compassHeading;

    [ObservableProperty]
    private string compassHeadingText = "Compass not started";

    [ObservableProperty]
    private string compassDirection = "--";

    [ObservableProperty]
    private string compassStatusText = "Open this page to start compass monitoring.";

    [ObservableProperty]
    private bool isAccelerometerSupported;

    [ObservableProperty]
    private bool isAccelerometerMonitoring;

    [ObservableProperty]
    private string accelerometerXText = "X: 0.00";

    [ObservableProperty]
    private string accelerometerYText = "Y: 0.00";

    [ObservableProperty]
    private string accelerometerZText = "Z: 0.00";

    [ObservableProperty]
    private string accelerometerMagnitudeText = "Magnitude: 0.00";

    [ObservableProperty]
    private string accelerometerStatusText = "Open this page to start accelerometer monitoring.";

    public SettingsViewModel(
        IAppSettingsService appSettingsService,
        IAppAppearanceService appAppearanceService,
        ICompassService compassService,
        IAccelerometerService accelerometerService)
    {
        _appSettingsService = appSettingsService;
        _appAppearanceService = appAppearanceService;
        _compassService = compassService;
        _accelerometerService = accelerometerService;
        _compassService.ReadingChanged += OnCompassReadingChanged;
        _accelerometerService.ReadingChanged += OnAccelerometerReadingChanged;
        IsCompassSupported = _compassService.IsSupported;
        IsAccelerometerSupported = _accelerometerService.IsSupported;
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
            _appAppearanceService.Apply(settings);
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
            _appAppearanceService.Apply(settings);
            ApplyMessage = "Settings saved.";
        }
        catch
        {
            ApplyMessage = "Failed to save settings. Please retry.";
        }
    }

    [RelayCommand]
    private void StartCompass()
    {
        IsCompassSupported = _compassService.IsSupported;
        if (!IsCompassSupported)
        {
            CompassStatusText = "Compass is not supported on this device or emulator.";
            IsCompassMonitoring = false;
            return;
        }

        _compassService.Start();
        IsCompassMonitoring = _compassService.IsMonitoring;
        CompassStatusText = IsCompassMonitoring
            ? "Compass is active. Rotate Yaw or Magnetic field in Emulator > Extended controls."
            : "Compass could not start on this target.";
    }

    [RelayCommand]
    private void StopCompass()
    {
        _compassService.Stop();
        IsCompassMonitoring = _compassService.IsMonitoring;
        if (IsCompassSupported)
        {
            CompassStatusText = "Compass stopped.";
        }
    }

    [RelayCommand]
    private void StartAccelerometer()
    {
        IsAccelerometerSupported = _accelerometerService.IsSupported;
        if (!IsAccelerometerSupported)
        {
            AccelerometerStatusText = "Accelerometer is not supported on this device or emulator.";
            IsAccelerometerMonitoring = false;
            return;
        }

        _accelerometerService.Start();
        IsAccelerometerMonitoring = _accelerometerService.IsMonitoring;
        AccelerometerStatusText = IsAccelerometerMonitoring
            ? "Accelerometer is active. Adjust Device Pose in Emulator > Extended controls."
            : "Accelerometer could not start on this target.";
    }

    [RelayCommand]
    private void StopAccelerometer()
    {
        _accelerometerService.Stop();
        IsAccelerometerMonitoring = _accelerometerService.IsMonitoring;
        if (IsAccelerometerSupported)
        {
            AccelerometerStatusText = "Accelerometer stopped.";
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

    private void OnCompassReadingChanged(object? sender, CompassReadingChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CompassHeading = e.Heading;
            CompassHeadingText = $"{e.Heading:F0}°";
            CompassDirection = ToCardinal(e.Heading);
            CompassStatusText = "Compass reading updated from device sensors.";
            IsCompassMonitoring = _compassService.IsMonitoring;
        });
    }

    private static string ToCardinal(double heading)
    {
        var normalized = ((heading % 360) + 360) % 360;
        return normalized switch
        {
            >= 337.5 or < 22.5 => "North",
            >= 22.5 and < 67.5 => "North-East",
            >= 67.5 and < 112.5 => "East",
            >= 112.5 and < 157.5 => "South-East",
            >= 157.5 and < 202.5 => "South",
            >= 202.5 and < 247.5 => "South-West",
            >= 247.5 and < 292.5 => "West",
            _ => "North-West"
        };
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerReadingChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AccelerometerXText = $"X: {e.X:F2}";
            AccelerometerYText = $"Y: {e.Y:F2}";
            AccelerometerZText = $"Z: {e.Z:F2}";
            var magnitude = Math.Sqrt((e.X * e.X) + (e.Y * e.Y) + (e.Z * e.Z));
            AccelerometerMagnitudeText = $"Magnitude: {magnitude:F2}";
            AccelerometerStatusText = "Accelerometer reading updated from device sensors.";
            IsAccelerometerMonitoring = _accelerometerService.IsMonitoring;
        });
    }
}
