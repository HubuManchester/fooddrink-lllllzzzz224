namespace NutriVision.Services.Abstractions;

public interface ISpeechService
{
    Task<bool> SpeakAsync(string text, CancellationToken ct);
}
