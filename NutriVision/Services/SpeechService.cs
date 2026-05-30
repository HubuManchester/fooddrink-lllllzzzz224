using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class SpeechService : ISpeechService
{
    public async Task SpeakAsync(string text, CancellationToken ct)
    {
        var options = new SpeechOptions
        {
            Pitch = 1.0f,
            Volume = 1.0f
        };

        await TextToSpeech.SpeakAsync(text, options, ct);
    }
}

