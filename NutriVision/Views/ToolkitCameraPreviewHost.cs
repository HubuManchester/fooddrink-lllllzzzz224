using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Views;

public sealed class ToolkitCameraPreviewHost : ICameraPreviewHost
{
    private readonly CameraView _cameraView;

    public ToolkitCameraPreviewHost(CameraView cameraView)
    {
        _cameraView = cameraView;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        var selectedCamera = await EnsureSelectedCameraAsync(ct);
        return _cameraView.IsAvailable && selectedCamera is not null;
    }

    public async Task<CameraPhoto?> CaptureAsync(CancellationToken ct)
    {
        await EnsureSelectedCameraAsync(ct);

        var captured = new TaskCompletionSource<CameraPhoto?>(TaskCreationOptions.RunContinuationsAsynchronously);

        async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
        {
            try
            {
                var filePath = Path.Combine(FileSystem.CacheDirectory, $"scan_{DateTime.UtcNow:yyyyMMddHHmmssfff}.jpg");
                await using (e.Media)
                await using (var destination = File.Create(filePath))
                {
                    await e.Media.CopyToAsync(destination, ct);
                }

                var stream = File.OpenRead(filePath);
                captured.TrySetResult(new CameraPhoto
                {
                    Path = filePath,
                    Content = stream
                });
            }
            catch (Exception ex)
            {
                captured.TrySetException(ex);
            }
        }

        void OnMediaCaptureFailed(object? sender, MediaCaptureFailedEventArgs e)
        {
            captured.TrySetException(new InvalidOperationException(e.FailureReason));
        }

        _cameraView.MediaCaptured += OnMediaCaptured;
        _cameraView.MediaCaptureFailed += OnMediaCaptureFailed;

        try
        {
            await _cameraView.CaptureImage(ct);
            return await captured.Task.WaitAsync(ct);
        }
        finally
        {
            _cameraView.MediaCaptured -= OnMediaCaptured;
            _cameraView.MediaCaptureFailed -= OnMediaCaptureFailed;
        }
    }

    public async Task<bool> CanToggleFlashAsync(CancellationToken ct)
    {
        var selectedCamera = await EnsureSelectedCameraAsync(ct);
        return selectedCamera?.IsFlashSupported == true;
    }

    public Task SetFlashAsync(bool enabled, CancellationToken ct)
    {
        _ = ct;
        _cameraView.CameraFlashMode = enabled ? CameraFlashMode.On : CameraFlashMode.Off;
        _cameraView.IsTorchOn = enabled;
        return Task.CompletedTask;
    }

    public async Task StartPreviewAsync(CancellationToken ct)
    {
        await EnsureSelectedCameraAsync(ct);
        await _cameraView.StartCameraPreview(ct);
    }

    public Task StopPreviewAsync(CancellationToken ct)
    {
        _ = ct;
        _cameraView.StopCameraPreview();
        return Task.CompletedTask;
    }

    private async Task<CameraInfo?> EnsureSelectedCameraAsync(CancellationToken ct)
    {
        if (_cameraView.SelectedCamera is not null)
        {
            return _cameraView.SelectedCamera;
        }

        var cameras = await _cameraView.GetAvailableCameras(ct);
        var selected = cameras.FirstOrDefault(camera => camera.Position == CameraPosition.Rear)
            ?? cameras.FirstOrDefault();

        _cameraView.SelectedCamera = selected;
        return selected;
    }
}
