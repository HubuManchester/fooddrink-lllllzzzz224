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

    [ObservableProperty]
    private string todayCalories = "0 kcal";

    [ObservableProperty]
    private string latestSummary = "暂无记录";

    public HomeViewModel(IHistoryRepository historyRepository, ISpeechService speechService, IHapticService hapticService)
    {
        _historyRepository = historyRepository;
        _speechService = speechService;
        _hapticService = hapticService;
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
                ? "暂无记录"
                : string.Join(" | ", latest.Select(x => $"{x.RecognizedFood}:{x.Calories:F0}"));
        }
        catch (Exception)
        {
            ErrorMessage = "加载失败，请稍后重试。";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SpeakSummaryAsync()
    {
        await _speechService.SpeakAsync($"今日热量 {TodayCalories}，最近记录：{LatestSummary}", CancellationToken.None);
        _hapticService.NotifySuccess();
    }
}

