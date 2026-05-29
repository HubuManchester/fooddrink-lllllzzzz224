namespace NutriVision.Services.Abstractions;

public interface ISpeechService
{
    Task SpeakAsync(string text, CancellationToken ct);
}

