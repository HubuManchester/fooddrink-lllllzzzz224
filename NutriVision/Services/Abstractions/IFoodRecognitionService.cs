using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface IFoodRecognitionService
{
    Task<RecognitionResult> RecognizeAsync(Stream photoStream, CancellationToken ct);
}

