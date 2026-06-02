using NutriVision.Models;
using NutriVision.Services.Abstractions;
using System.Diagnostics;

namespace NutriVision.Services;

public sealed class CameraService : ICameraService
{
    private bool _flashEnabled;
    private ICameraPreviewHost? _previewHost;

    public async Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        if (_previewHost is not null)
        {
            var permission = await EnsureCameraPermissionAsync();
            return permission == PermissionStatus.Granted && await _previewHost.IsAvailableAsync(ct);
        }

        return MediaPicker.Default.IsCaptureSupported;
    }

    public async Task<CameraPhoto?> CaptureAsync(CancellationToken ct)
    {
        if (_previewHost is not null)
        {
            var permission = await EnsureCameraPermissionAsync();
            if (permission != PermissionStatus.Granted)
            {
                return null;
            }

            return await _previewHost.CaptureAsync(ct);
        }

        if (!MediaPicker.Default.IsCaptureSupported)
        {
            return null;
        }

        try
        {
            var permission = await EnsureCameraPermissionAsync();
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
        if (_previewHost is null)
        {
            return Task.FromResult(false);
        }

        return _previewHost.CanToggleFlashAsync(ct);
    }

    public async Task SetFlashAsync(bool enabled, CancellationToken ct)
    {
        _flashEnabled = enabled;

        if (_previewHost is not null)
        {
            await _previewHost.SetFlashAsync(enabled, ct);
        }
    }

    public void AttachPreviewHost(ICameraPreviewHost previewHost)
    {
        _previewHost = previewHost;
    }

    public void DetachPreviewHost()
    {
        _previewHost = null;
    }

    public async Task StartPreviewAsync(CancellationToken ct)
    {
        if (_previewHost is null)
        {
            return;
        }

        var permission = await EnsureCameraPermissionAsync();
        if (permission != PermissionStatus.Granted)
        {
            return;
        }

        await _previewHost.StartPreviewAsync(ct);
        if (_flashEnabled)
        {
            await _previewHost.SetFlashAsync(true, ct);
        }
    }

    public Task StopPreviewAsync(CancellationToken ct)
    {
        if (_previewHost is null)
        {
            return Task.CompletedTask;
        }

        return _previewHost.StopPreviewAsync(ct);
    }

    private static async Task<PermissionStatus> EnsureCameraPermissionAsync()
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
        {
            permission = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return permission;
    }
}
