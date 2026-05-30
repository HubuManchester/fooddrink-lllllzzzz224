using NutriVision.Models;
using NutriVision.Services.Abstractions;
using SQLite;

namespace NutriVision.Services;

public sealed class HistoryRepository : IHistoryRepository
{
    private readonly SQLiteAsyncConnection _db;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public HistoryRepository()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "nutrivision.db3");
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task AddAsync(ScanSession session, CancellationToken ct)
    {
        await EnsureReadyAsync(ct);
        ct.ThrowIfCancellationRequested();
        await _db.InsertAsync(session);
    }

    public async Task<IReadOnlyList<ScanSession>> QueryAsync(HistoryQuery query, CancellationToken ct)
    {
        await EnsureReadyAsync(ct);
        ct.ThrowIfCancellationRequested();

        var records = await _db.Table<ScanSession>().ToListAsync();
        var filtered = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.NameKeyword))
        {
            filtered = filtered.Where(x => (x.RecognizedFood ?? string.Empty).Contains(query.NameKeyword, StringComparison.OrdinalIgnoreCase));
        }

        if (query.DateFromUtc.HasValue)
        {
            filtered = filtered.Where(x => x.TimestampUtc >= query.DateFromUtc.Value);
        }

        if (query.DateToUtc.HasValue)
        {
            filtered = filtered.Where(x => x.TimestampUtc <= query.DateToUtc.Value);
        }

        if (query.MinCalories.HasValue)
        {
            filtered = filtered.Where(x => x.Calories >= query.MinCalories.Value);
        }

        if (query.MaxCalories.HasValue)
        {
            filtered = filtered.Where(x => x.Calories <= query.MaxCalories.Value);
        }

        return filtered
            .OrderByDescending(x => x.TimestampUtc)
            .Take(Math.Max(1, query.Limit))
            .ToList();
    }

    public async Task<IReadOnlyList<DailyCaloriesPoint>> GetDailyCaloriesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var rows = await QueryAsync(new HistoryQuery
        {
            DateFromUtc = fromUtc,
            DateToUtc = toUtc,
            Limit = 5000
        }, ct);

        return rows
            .GroupBy(x => x.TimestampUtc.Date)
            .Select(g => new DailyCaloriesPoint
            {
                DayUtc = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                Calories = g.Sum(x => x.Calories)
            })
            .OrderBy(x => x.DayUtc)
            .ToList();
    }

    public async Task<MacroRatio> GetMacroRatioAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var rows = await QueryAsync(new HistoryQuery
        {
            DateFromUtc = fromUtc,
            DateToUtc = toUtc,
            Limit = 5000
        }, ct);

        var protein = rows.Sum(x => x.Protein);
        var fat = rows.Sum(x => x.Fat);
        var carbs = rows.Sum(x => x.Carbs);
        var total = protein + fat + carbs;

        if (total <= 0.001)
        {
            return new MacroRatio();
        }

        return new MacroRatio
        {
            ProteinPercent = protein / total * 100,
            FatPercent = fat / total * 100,
            CarbsPercent = carbs / total * 100
        };
    }

    private async Task EnsureReadyAsync(CancellationToken ct)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized)
            {
                return;
            }

            await _db.CreateTableAsync<ScanSession>();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}

