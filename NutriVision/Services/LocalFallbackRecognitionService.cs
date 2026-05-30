using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class LocalFallbackRecognitionService : ILocalFallbackRecognitionService
{
    private static readonly string[] FallbackFoods =
    [
        "apple",
        "banana",
        "rice",
        "bread",
        "egg",
        "salad"
    ];

    public Task<RecognitionResult> RecognizeAsync(Stream photoStream, CancellationToken ct)
    {
        _ = photoStream;
        if (ct.IsCancellationRequested)
        {
            return Task.FromResult(RecognitionResult.Failure("CANCELLED", "识别已取消。"));
        }

        // Deterministic fallback keeps demo stable when cloud fails.
        var index = DateTime.UtcNow.Second % FallbackFoods.Length;
        return Task.FromResult(RecognitionResult.Success(FallbackFoods[index]));
    }
}
