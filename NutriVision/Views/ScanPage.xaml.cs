using NutriVision.ViewModels;
using NutriVision.Services.Abstractions;

namespace NutriVision.Views;

public partial class ScanPage : ContentPage
{
    private readonly ScanViewModel _viewModel;
    private readonly ICameraService _cameraService;
    private ToolkitCameraPreviewHost? _previewHost;

    public ScanPage(ScanViewModel viewModel, ICameraService cameraService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _cameraService = cameraService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _previewHost ??= new ToolkitCameraPreviewHost(PreviewCamera);
        _cameraService.AttachPreviewHost(_previewHost);
        await _viewModel.InitializeCameraCommand.ExecuteAsync(null);
    }

    protected override async void OnDisappearing()
    {
        await _viewModel.StopCameraCommand.ExecuteAsync(null);
        _cameraService.DetachPreviewHost();
        base.OnDisappearing();
    }
}
