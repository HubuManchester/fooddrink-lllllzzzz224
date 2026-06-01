using System.Diagnostics;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class SpeechService : ISpeechService
{
    private readonly IAppSettingsService _appSettingsService;

    public SpeechService(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
    }

    public async Task<bool> SpeakAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        AppSettings settings;
        try
        {
            settings = await _appSettingsService.LoadAsync(ct);
        }
        catch
        {
            settings = new AppSettings();
        }

        if (!settings.TtsEnabled)
        {
            return false;
        }

        var options = new SpeechOptions
        {
            Pitch = 1.0f,
            Volume = 1.0f
        };

        try
        {
            await TextToSpeech.SpeakAsync(text, options, ct);
            return true;
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"[SpeechService] TTS engine unavailable: {ex.Message}");
            return false;
        }
        catch (FeatureNotSupportedException ex)
        {
            Debug.WriteLine($"[SpeechService] TTS unsupported: {ex.Message}");
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Keep speech failures non-fatal for scan/history flow.
            Debug.WriteLine($"[SpeechService] TTS failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}
