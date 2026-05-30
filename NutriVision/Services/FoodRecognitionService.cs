using NutriVision.Helpers;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class FoodRecognitionService : IFoodRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalFallbackRecognitionService _fallbackService;

    public FoodRecognitionService(HttpClient httpClient, ILocalFallbackRecognitionService fallbackService)
    {
        _httpClient = httpClient;
        _fallbackService = fallbackService;
    }

    public async Task<RecognitionResult> RecognizeAsync(Stream photoStream, CancellationToken ct)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            // Placeholder cloud call. It intentionally falls back unless response contains a food name.
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(photoStream);
            content.Add(streamContent, "file", "photo.jpg");

            using var response = await _httpClient.PostAsync("/recognize", content, linked.Token);
            if (response.IsSuccessStatusCode)
            {
                var name = (await response.Content.ReadAsStringAsync(linked.Token)).Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return RecognitionResult.Success(name);
                }
            }
        }
        catch
        {
            // Cloud failures are expected in demo environments. Fall through to fallback.
        }

        if (photoStream.CanSeek)
        {
            photoStream.Position = 0;
        }

        var fallbackResult = await _fallbackService.RecognizeAsync(photoStream, ct);
        if (fallbackResult.IsSuccess)
        {
            return fallbackResult;
        }

        return RecognitionResult.Failure("RECOGNITION_FAILED", ErrorMessages.RecognitionFailed);
    }
}

