using System.Text.Json;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private const string PreferencesKey = "nutrivision_app_settings_v1";

    public Task<AppSettings> LoadAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var raw = Preferences.Default.Get(PreferencesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Task.FromResult(new AppSettings());
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<AppSettings>(raw);
            return Task.FromResult(parsed ?? new AppSettings());
        }
        catch
        {
            return Task.FromResult(new AppSettings());
        }
    }

    public Task SaveAsync(AppSettings settings, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var raw = JsonSerializer.Serialize(settings);
        Preferences.Default.Set(PreferencesKey, raw);
        return Task.CompletedTask;
    }
}
