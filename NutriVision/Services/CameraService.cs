using NutriVision.Models;
using NutriVision.Services.Abstractions;
using System.Diagnostics;

namespace NutriVision.Services;

public sealed class CameraService : ICameraService
{
    private bool _flashEnabled;

    public Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(MediaPicker.Default.IsCaptureSupported);
    }

    public async Task<CameraPhoto?> CaptureAsync(CancellationToken ct)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            return null;
        }

        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permission != PermissionStatus.Granted)
            {
                permission = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (permission != PermissionStatus.Granted)
            {
                return null;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                return null;
            }

            var filePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            await using (var source = await photo.OpenReadAsync())
            await using (var destination = File.OpenWrite(filePath))
            {
                await source.CopyToAsync(destination, ct);
            }

            var stream = File.OpenRead(filePath);
            return new CameraPhoto
            {
                Path = filePath,
                Content = stream
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Debug.WriteLine($"[CameraService] Capture failed: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public Task<bool> CanToggleFlashAsync(CancellationToken ct)
    {
        _ = ct;
        // MediaPicker does not expose flashlight control. Keep capability contract for later CameraView integration.
        return Task.FromResult(false);
    }

    public Task SetFlashAsync(bool enabled, CancellationToken ct)
    {
        _ = ct;
        _flashEnabled = enabled;
        return Task.CompletedTask;
    }
}
