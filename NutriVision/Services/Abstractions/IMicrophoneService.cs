namespace NutriVision.Services.Abstractions;

public interface IMicrophoneService
{
    Task<VoiceInputResult> ListenForFoodNameAsync(CancellationToken ct);
}
