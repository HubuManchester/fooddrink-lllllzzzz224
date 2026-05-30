using NutriVision.Helpers;
using NutriVision.Models;
using NutriVision.Services.Abstractions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NutriVision.Services;

public sealed class FoodRecognitionService : IFoodRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalFallbackRecognitionService _fallbackService;
    private readonly FoodRecognitionOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public FoodRecognitionService(
        HttpClient httpClient,
        ILocalFallbackRecognitionService fallbackService,
        FoodRecognitionOptions options)
    {
        _httpClient = httpClient;
        _fallbackService = fallbackService;
        _options = options;
    }

    public async Task<RecognitionResult> RecognizeAsync(Stream photoStream, CancellationToken ct)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        try
        {
            // Cloud API is optional for demo stability. If token is missing, we skip cloud and use local fallback.
            if (!string.IsNullOrWhiteSpace(_options.ApiToken))
            {
                var recognized = await TryCloudRecognitionAsync(photoStream, linked.Token);
                if (!string.IsNullOrWhiteSpace(recognized))
                {
                    return RecognitionResult.Success(recognized);
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

    private async Task<string?> TryCloudRecognitionAsync(Stream photoStream, CancellationToken ct)
    {
        if (photoStream.CanSeek)
        {
            photoStream.Position = 0;
        }

        await using var buffer = new MemoryStream();
        await photoStream.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();
        if (bytes.Length == 0)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ApiUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);

        using var body = new ByteArrayContent(bytes);
        body.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Content = body;

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<CloudLabel>>(payload, JsonOptions);
            var label = items?
                .OrderByDescending(x => x.Score)
                .Select(x => NormalizeLabel(x.Label))
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return string.IsNullOrWhiteSpace(label) ? null : label;
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        var normalized = label.Trim().ToLowerInvariant().Replace('_', ' ').Replace('-', ' ');
        if (normalized.Contains("apple", StringComparison.Ordinal))
        {
            return "apple";
        }

        if (normalized.Contains("banana", StringComparison.Ordinal))
        {
            return "banana";
        }

        if (normalized.Contains("rice", StringComparison.Ordinal))
        {
            return "rice";
        }

        if (normalized.Contains("bread", StringComparison.Ordinal))
        {
            return "bread";
        }

        if (normalized.Contains("egg", StringComparison.Ordinal) || normalized.Contains("omelet", StringComparison.Ordinal) || normalized.Contains("omelette", StringComparison.Ordinal))
        {
            return "egg";
        }

        if (normalized.Contains("salad", StringComparison.Ordinal))
        {
            return "salad";
        }

        return null;
    }

    private sealed class CloudLabel
    {
        public string Label { get; init; } = string.Empty;
        public double Score { get; init; }
    }
}
