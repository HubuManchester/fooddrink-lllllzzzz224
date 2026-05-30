using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly IHistoryRepository _historyRepository;

    public ObservableCollection<ScanSession> Records { get; } = [];

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime toDate = DateTime.Today;

    [ObservableProperty]
    private string minCaloriesText = string.Empty;

    [ObservableProperty]
    private string maxCaloriesText = string.Empty;

    [ObservableProperty]
    private string resultSummary = "No records loaded.";

    [ObservableProperty]
    private bool isEmpty = true;

    public HistoryViewModel(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await ApplyFiltersAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            if (ToDate.Date < FromDate.Date)
            {
                ErrorMessage = "End date must be after start date.";
                return;
            }

            var query = new HistoryQuery
            {
                NameKeyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                DateFromUtc = DateTime.SpecifyKind(FromDate.Date, DateTimeKind.Local).ToUniversalTime(),
                DateToUtc = DateTime.SpecifyKind(ToDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime(),
                MinCalories = ParseNullableDouble(MinCaloriesText),
                MaxCalories = ParseNullableDouble(MaxCaloriesText),
                Limit = 500
            };

            if (query.MinCalories.HasValue && query.MaxCalories.HasValue && query.MinCalories > query.MaxCalories)
            {
                ErrorMessage = "Min calories cannot be greater than max calories.";
                return;
            }

            var rows = await _historyRepository.QueryAsync(query, CancellationToken.None);

            Records.Clear();
            foreach (var row in rows)
            {
                Records.Add(row);
            }

            IsEmpty = Records.Count == 0;
            ResultSummary = IsEmpty
                ? "No records matched current filters."
                : $"{Records.Count} records loaded.";
        }
        catch
        {
            ErrorMessage = "Failed to load history. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchKeyword = string.Empty;
        MinCaloriesText = string.Empty;
        MaxCaloriesText = string.Empty;
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;
        await ApplyFiltersAsync();
    }

    private static double? ParseNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value.Trim(), out var number) ? number : null;
    }
}
