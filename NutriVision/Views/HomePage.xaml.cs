using NutriVision.ViewModels;

namespace NutriVision.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_viewModel.IsBusy)
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
