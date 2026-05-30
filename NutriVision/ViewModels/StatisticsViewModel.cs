using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly IHistoryRepository _historyRepository;

    public ObservableCollection<DailyCaloriesBarItem> DailyTrend { get; } = [];

    [ObservableProperty]
    private string totalCaloriesText = "0 kcal";

    [ObservableProperty]
    private string dateRangeText = "Last 7 days";

    [ObservableProperty]
    private bool isEmpty = true;

    [ObservableProperty]
    private double proteinPercent;

    [ObservableProperty]
    private double fatPercent;

    [ObservableProperty]
    private double carbsPercent;

    [ObservableProperty]
    private double proteinProgress;

    [ObservableProperty]
    private double fatProgress;

    [ObservableProperty]
    private double carbsProgress;

    public StatisticsViewModel(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var localToday = DateTime.Today;
            var localStart = localToday.AddDays(-6);

            var fromUtc = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
            var toUtc = DateTime.SpecifyKind(localToday.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();

            DateRangeText = $"{localStart:yyyy-MM-dd} to {localToday:yyyy-MM-dd}";

            var dailyPoints = await _historyRepository.GetDailyCaloriesAsync(fromUtc, toUtc, CancellationToken.None);
            var ratios = await _historyRepository.GetMacroRatioAsync(fromUtc, toUtc, CancellationToken.None);

            var dailyByLocalDate = dailyPoints
                .GroupBy(x => x.DayUtc.ToLocalTime().Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Calories));

            var chartRows = new List<DailyCaloriesBarItem>();
            for (var day = localStart; day <= localToday; day = day.AddDays(1))
            {
                var calories = dailyByLocalDate.TryGetValue(day.Date, out var value) ? value : 0d;
                chartRows.Add(new DailyCaloriesBarItem(day, calories, 0d));
            }

            var maxCalories = Math.Max(1d, chartRows.Max(x => x.Calories));
            DailyTrend.Clear();
            foreach (var row in chartRows)
            {
                var progress = row.Calories <= 0 ? 0d : row.Calories / maxCalories;
                DailyTrend.Add(new DailyCaloriesBarItem(row.DayLocal, row.Calories, progress));
            }

            var totalCalories = chartRows.Sum(x => x.Calories);
            TotalCaloriesText = $"{totalCalories:F0} kcal";
            IsEmpty = totalCalories <= 0.001;

            ProteinPercent = ratios.ProteinPercent;
            FatPercent = ratios.FatPercent;
            CarbsPercent = ratios.CarbsPercent;
            ProteinProgress = Clamp01(ratios.ProteinPercent / 100d);
            FatProgress = Clamp01(ratios.FatPercent / 100d);
            CarbsProgress = Clamp01(ratios.CarbsPercent / 100d);
        }
        catch
        {
            ErrorMessage = "Failed to load statistics. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static double Clamp01(double value)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > 1)
        {
            return 1;
        }

        return value;
    }

}
