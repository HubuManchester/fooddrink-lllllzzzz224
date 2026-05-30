namespace NutriVision.Services;

public sealed class FoodRecognitionOptions
{
    public string ApiUrl { get; init; } = "https://api-inference.huggingface.co/models/nateraw/food";
    public string? ApiToken { get; init; }
}
