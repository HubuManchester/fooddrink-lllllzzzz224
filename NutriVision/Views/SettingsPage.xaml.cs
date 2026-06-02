using NutriVision.ViewModels;

namespace NutriVision.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartCompassCommand.Execute(null);
        _viewModel.StartAccelerometerCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopAccelerometerCommand.Execute(null);
        _viewModel.StopCompassCommand.Execute(null);
        base.OnDisappearing();
    }
}
