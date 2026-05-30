using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class AppAppearanceService : IAppAppearanceService
{
    public void Apply(AppSettings settings)
    {
        if (Application.Current?.Resources is null)
        {
            return;
        }

        Application.Current.UserAppTheme = settings.ThemeMode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        ApplyFontScale(settings.FontScale);
        ApplyContrast(settings.HighContrastEnabled);
    }

    private static void ApplyFontScale(FontScale scale)
    {
        var app = Application.Current!;
        var body = scale switch
        {
            FontScale.Small => 12d,
            FontScale.Large => 18d,
            _ => 14d
        };

        var sub = scale switch
        {
            FontScale.Small => 20d,
            FontScale.Large => 30d,
            _ => 24d
        };

        var head = scale switch
        {
            FontScale.Small => 26d,
            FontScale.Large => 38d,
            _ => 32d
        };

        app.Resources["AppFontSize"] = body;
        app.Resources["AppSubHeadlineFontSize"] = sub;
        app.Resources["AppHeadlineFontSize"] = head;
    }

    private static void ApplyContrast(bool highContrast)
    {
        var app = Application.Current!;
        if (highContrast)
        {
            app.Resources["AppPageBackgroundColor"] = Color.FromArgb("#000000");
            app.Resources["AppTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppSecondaryTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppButtonBackgroundColor"] = Color.FromArgb("#FFD800");
            app.Resources["AppButtonTextColor"] = Color.FromArgb("#000000");
            app.Resources["AppErrorTextColor"] = Color.FromArgb("#FF6B6B");
            app.Resources["AppSuccessTextColor"] = Color.FromArgb("#7CFF7C");
            return;
        }

        var isDark = app.UserAppTheme == AppTheme.Dark ||
                     (app.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);

        if (isDark)
        {
            app.Resources["AppPageBackgroundColor"] = Color.FromArgb("#141414");
            app.Resources["AppTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppSecondaryTextColor"] = Color.FromArgb("#C8C8C8");
            app.Resources["AppButtonBackgroundColor"] = Color.FromArgb("#AC99EA");
            app.Resources["AppButtonTextColor"] = Color.FromArgb("#242424");
            app.Resources["AppErrorTextColor"] = Color.FromArgb("#FF8A80");
            app.Resources["AppSuccessTextColor"] = Color.FromArgb("#9BE7A0");
        }
        else
        {
            app.Resources["AppPageBackgroundColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppTextColor"] = Color.FromArgb("#000000");
            app.Resources["AppSecondaryTextColor"] = Color.FromArgb("#212121");
            app.Resources["AppButtonBackgroundColor"] = Color.FromArgb("#512BD4");
            app.Resources["AppButtonTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppErrorTextColor"] = Color.FromArgb("#D32F2F");
            app.Resources["AppSuccessTextColor"] = Color.FromArgb("#2E7D32");
        }
    }
}
