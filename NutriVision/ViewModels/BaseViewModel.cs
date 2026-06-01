using CommunityToolkit.Mvvm.ComponentModel;

namespace NutriVision.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;
}
