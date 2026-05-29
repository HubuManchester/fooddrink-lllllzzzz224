using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface IHistoryRepository
{
    Task AddAsync(ScanSession session, CancellationToken ct);
    Task<IReadOnlyList<ScanSession>> QueryAsync(HistoryQuery query, CancellationToken ct);
    Task<IReadOnlyList<DailyCaloriesPoint>> GetDailyCaloriesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct);
    Task<MacroRatio> GetMacroRatioAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}

