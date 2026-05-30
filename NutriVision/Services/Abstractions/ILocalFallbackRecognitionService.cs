using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface ILocalFallbackRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(Stream photoStream, CancellationToken ct);
}

