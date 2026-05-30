namespace NutriVision.Models;

public sealed class HistoryQuery
{
    public string? NameKeyword { get; init; }
    public DateTime? DateFromUtc { get; init; }
    public DateTime? DateToUtc { get; init; }
    public double? MinCalories { get; init; }
    public double? MaxCalories { get; init; }
    public int Limit { get; init; } = 100;
}

