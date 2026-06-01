using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IAppAppearanceService _appAppearanceService;
    private readonly IHistoryRepository _historyRepository;

    public App(
        AppShell shell,
        IAppSettingsService appSettingsService,
        IAppAppearanceService appAppearanceService,
        IHistoryRepository historyRepository)
    {
        InitializeComponent();
        _shell = shell;
        _appSettingsService = appSettingsService;
        _appAppearanceService = appAppearanceService;
        _historyRepository = historyRepository;
        _ = InitializeAppAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }

    private async Task InitializeAppAsync()
    {
        await RestoreSettingsAsync();
        await EnsureDemoDataAsync();
    }

    private async Task RestoreSettingsAsync()
    {
        try
        {
            var settings = await _appSettingsService.LoadAsync(CancellationToken.None);
            _appAppearanceService.Apply(settings);
        }
        catch
        {
            // Keep system defaults when loading persisted settings fails.
        }
    }

    private async Task EnsureDemoDataAsync()
    {
        try
        {
            var existing = await _historyRepository.QueryAsync(new HistoryQuery { Limit = 1 }, CancellationToken.None);
            if (existing.Count > 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var demoRows = new List<ScanSession>
            {
                CreateDemo("apple", 95, 0.5, 0.3, 25, "Campus Cafe", now.AddDays(-6).AddHours(8)),
                CreateDemo("egg", 155, 13, 11, 1.1, "Dorm Kitchen", now.AddDays(-6).AddHours(12)),
                CreateDemo("rice", 206, 4.3, 0.4, 45, "Student Canteen", now.AddDays(-5).AddHours(13)),
                CreateDemo("salad", 120, 3.2, 8.0, 11, "Green Bowl", now.AddDays(-4).AddHours(19)),
                CreateDemo("bread", 160, 5.2, 2.1, 30, "Bakery", now.AddDays(-4).AddHours(8)),
                CreateDemo("banana", 105, 1.3, 0.4, 27, "Library Snack Bar", now.AddDays(-3).AddHours(15)),
                CreateDemo("rice", 230, 5.0, 0.5, 50, "Food Court", now.AddDays(-2).AddHours(18)),
                CreateDemo("egg", 78, 6.3, 5.3, 0.6, "Home", now.AddDays(-1).AddHours(7)),
                CreateDemo("apple", 95, 0.5, 0.3, 25, "Gym Vending Area", now.AddHours(-6)),
                CreateDemo("salad", 135, 4.2, 9.1, 12, "Campus Cafe", now.AddHours(-2))
            };

            foreach (var row in demoRows)
            {
                await _historyRepository.AddAsync(row, CancellationToken.None);
            }
        }
        catch
        {
            // Demo seed should never block app startup.
        }
    }

    private static ScanSession CreateDemo(
        string foodName,
        double calories,
        double protein,
        double fat,
        double carbs,
        string location,
        DateTime timestampUtc)
    {
        return new ScanSession
        {
            RecognizedFood = foodName,
            Calories = calories,
            Protein = protein,
            Fat = fat,
            Carbs = carbs,
            Location = location,
            TimestampUtc = DateTime.SpecifyKind(timestampUtc, DateTimeKind.Utc),
            Status = ScanStatus.Success
        };
    }
}
