using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface ICameraService
{
    Task<bool> IsAvailableAsync(CancellationToken ct);
    Task<CameraPhoto?> CaptureAsync(CancellationToken ct);
    Task<bool> CanToggleFlashAsync(CancellationToken ct);
    Task SetFlashAsync(bool enabled, CancellationToken ct);
    void AttachPreviewHost(ICameraPreviewHost previewHost);
    void DetachPreviewHost();
    Task StartPreviewAsync(CancellationToken ct);
    Task StopPreviewAsync(CancellationToken ct);
}

public interface ICameraPreviewHost
{
    Task<bool> IsAvailableAsync(CancellationToken ct);
    Task<CameraPhoto?> CaptureAsync(CancellationToken ct);
    Task<bool> CanToggleFlashAsync(CancellationToken ct);
    Task SetFlashAsync(bool enabled, CancellationToken ct);
    Task StartPreviewAsync(CancellationToken ct);
    Task StopPreviewAsync(CancellationToken ct);
}
