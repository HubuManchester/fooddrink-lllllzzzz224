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
            app.Resources["AppCardBackgroundColor"] = Color.FromArgb("#0F0F0F");
            app.Resources["AppCardBorderColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppChipBackgroundColor"] = Color.FromArgb("#FFD800");
            app.Resources["AppChipTextColor"] = Color.FromArgb("#000000");
            return;
        }

        var isDark = app.UserAppTheme == AppTheme.Dark ||
                     (app.UserAppTheme == AppTheme.Unspecified && app.RequestedTheme == AppTheme.Dark);

        if (isDark)
        {
            app.Resources["AppPageBackgroundColor"] = Color.FromArgb("#0F1115");
            app.Resources["AppTextColor"] = Color.FromArgb("#F7F9FC");
            app.Resources["AppSecondaryTextColor"] = Color.FromArgb("#AEB7C2");
            app.Resources["AppButtonBackgroundColor"] = Color.FromArgb("#0A84FF");
            app.Resources["AppButtonTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppErrorTextColor"] = Color.FromArgb("#FF8A80");
            app.Resources["AppSuccessTextColor"] = Color.FromArgb("#6EE787");
            app.Resources["AppCardBackgroundColor"] = Color.FromArgb("#1FD1D8E0");
            app.Resources["AppCardBorderColor"] = Color.FromArgb("#2FFFFFFF");
            app.Resources["AppChipBackgroundColor"] = Color.FromArgb("#1F0A84FF");
            app.Resources["AppChipTextColor"] = Color.FromArgb("#7EBBFF");
        }
        else
        {
            app.Resources["AppPageBackgroundColor"] = Color.FromArgb("#F3F6FB");
            app.Resources["AppTextColor"] = Color.FromArgb("#0F172A");
            app.Resources["AppSecondaryTextColor"] = Color.FromArgb("#64748B");
            app.Resources["AppButtonBackgroundColor"] = Color.FromArgb("#0A84FF");
            app.Resources["AppButtonTextColor"] = Color.FromArgb("#FFFFFF");
            app.Resources["AppErrorTextColor"] = Color.FromArgb("#D32F2F");
            app.Resources["AppSuccessTextColor"] = Color.FromArgb("#2E7D32");
            app.Resources["AppCardBackgroundColor"] = Color.FromArgb("#CCFFFFFF");
            app.Resources["AppCardBorderColor"] = Color.FromArgb("#2EFFFFFF");
            app.Resources["AppChipBackgroundColor"] = Color.FromArgb("#120A84FF");
            app.Resources["AppChipTextColor"] = Color.FromArgb("#0A84FF");
        }
    }
}
