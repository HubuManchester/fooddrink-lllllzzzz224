namespace NutriVision.Services.Abstractions;

public enum VoiceInputFailureReason
{
    None = 0,
    PermissionDenied = 1,
    Unsupported = 2,
    NoSpeechDetected = 3,
    Unknown = 4
}

public sealed record VoiceInputResult(string? FoodName, VoiceInputFailureReason FailureReason)
{
    public bool IsSuccess => !string.IsNullOrWhiteSpace(FoodName) && FailureReason == VoiceInputFailureReason.None;

    public static VoiceInputResult Success(string foodName) => new(foodName, VoiceInputFailureReason.None);
    public static VoiceInputResult Fail(VoiceInputFailureReason reason) => new(null, reason);
}
