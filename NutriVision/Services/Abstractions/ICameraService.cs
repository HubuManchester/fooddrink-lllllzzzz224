using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface ICameraService
{
    Task<bool> IsAvailableAsync(CancellationToken ct);
    Task<CameraPhoto?> CaptureAsync(CancellationToken ct);
    Task<bool> CanToggleFlashAsync(CancellationToken ct);
    Task SetFlashAsync(bool enabled, CancellationToken ct);
}

