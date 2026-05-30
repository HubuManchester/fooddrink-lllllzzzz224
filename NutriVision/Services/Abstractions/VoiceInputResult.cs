namespace NutriVision.Services.Abstractions;

public enum VoiceInputFailureReason
{
    None = 0,
    PermissionDenied = 1,
    Unsupported = 2,
    NoSpeechDetected = 3,
    SpeechPrivacyDisabled = 4,
    NetworkUnavailable = 5,
    Unknown = 6
}

public sealed record VoiceInputResult(string? FoodName, VoiceInputFailureReason FailureReason, string? TechnicalMessage = null)
{
    public bool IsSuccess => !string.IsNullOrWhiteSpace(FoodName) && FailureReason == VoiceInputFailureReason.None;

    public static VoiceInputResult Success(string foodName) => new(foodName, VoiceInputFailureReason.None);
    public static VoiceInputResult Fail(VoiceInputFailureReason reason, string? technicalMessage = null) => new(null, reason, technicalMessage);
}
