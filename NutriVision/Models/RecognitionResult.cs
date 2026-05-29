namespace NutriVision.Models;

public sealed class RecognitionResult
{
    public bool IsSuccess { get; init; }
    public string? FoodName { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }

    public static RecognitionResult Success(string foodName) =>
        new() { IsSuccess = true, FoodName = foodName };

    public static RecognitionResult Failure(string errorCode, string message) =>
        new() { IsSuccess = false, ErrorCode = errorCode, Message = message };
}

