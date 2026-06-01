using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ISpeechService _speechService;
    private readonly IHapticService _hapticService;
    private readonly ILocationService _locationService;
    private CancellationTokenSource? _locationPopupCts;

    [ObservableProperty]
    private string todayCalories = "0 kcal";

    [ObservableProperty]
    private string latestSummary = "No records yet";

    [ObservableProperty]
    private string speechMessage = string.Empty;

    [ObservableProperty]
    private bool isSpeaking;

    [ObservableProperty]
    private string locationText = "Location not checked";

    [ObservableProperty]
    private bool isLocationPopupVisible;

    [ObservableProperty]
    private string locationPopupMessage = string.Empty;

    public HomeViewModel(
        IHistoryRepository historyRepository,
        ISpeechService speechService,
        IHapticService hapticService,
        ILocationService locationService)
    {
        _historyRepository = historyRepository;
        _speechService = speechService;
        _hapticService = hapticService;
        _locationService = locationService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(1).AddTicks(-1);
            var totals = await _historyRepository.GetDailyCaloriesAsync(from, to, CancellationToken.None);
            TodayCalories = $"{totals.Sum(x => x.Calories):F0} kcal";

            var latest = await _historyRepository.QueryAsync(new HistoryQuery { Limit = 3 }, CancellationToken.None);
            LatestSummary = latest.Count == 0
                ? "No records yet"
                : string.Join(" | ", latest.Select(x => $"{x.RecognizedFood}:{x.Calories:F0}"));
        }
        catch
        {
            ErrorMessage = "Failed to load home data. Please retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SpeakSummaryAsync()
    {
        if (IsSpeaking)
        {
            return;
        }

        ErrorMessage = null;
        SpeechMessage = "Speaking summary...";
        IsSpeaking = true;

        try
        {
            var spoken = await _speechService.SpeakAsync(
                $"Today calories {TodayCalories}. Latest records: {LatestSummary}",
                CancellationToken.None);

            if (!spoken)
            {
                SpeechMessage = string.Empty;
                ErrorMessage = "Speech is unavailable or disabled in Settings.";
                return;
            }

            SpeechMessage = "Summary spoken.";
            _hapticService.NotifySuccess();
        }
        catch
        {
            SpeechMessage = string.Empty;
            ErrorMessage = "Speech failed. Please try again.";
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    [RelayCommand]
    private async Task RefreshLocationAsync()
    {
        ErrorMessage = null;

        try
        {
            var address = await _locationService.GetCurrentAddressAsync(CancellationToken.None);
            if (string.IsNullOrWhiteSpace(address))
            {
                const string message = "Location unavailable. Please enable permission or set emulator location.";
                LocationText = message;
                ShowLocationPopup(message);
                return;
            }

            LocationText = address;
            ShowLocationPopup(address);
            _hapticService.NotifySuccess();
        }
        catch
        {
            const string message = "Failed to get location. Please retry.";
            LocationText = message;
            ShowLocationPopup(message);
        }
    }

    [RelayCommand]
    private void DismissLocationPopup()
    {
        _locationPopupCts?.Cancel();
        IsLocationPopupVisible = false;
    }

    private void ShowLocationPopup(string message)
    {
        _locationPopupCts?.Cancel();
        _locationPopupCts = new CancellationTokenSource();

        LocationPopupMessage = message;
        IsLocationPopupVisible = true;

        _ = HideLocationPopupLaterAsync(_locationPopupCts.Token);
    }

    private async Task HideLocationPopupLaterAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(6), token);
            await MainThread.InvokeOnMainThreadAsync(() => IsLocationPopupVisible = false);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
