namespace NutriVision.Models;

public sealed record DailyCaloriesBarItem(DateTime DayLocal, double Calories, double Progress)
{
    public string DayLabel => DayLocal.ToString("MM-dd");
    public string CaloriesLabel => $"{Calories:F0} kcal";
}
