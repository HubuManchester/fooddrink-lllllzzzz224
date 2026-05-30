namespace NutriVision.Services.Abstractions;

public interface IMicrophoneService
{
    Task<string?> ListenForFoodNameAsync(CancellationToken ct);
}

