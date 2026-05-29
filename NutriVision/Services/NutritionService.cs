using NutriVision.Models;
using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class NutritionService : INutritionService
{
    private static readonly Dictionary<string, NutritionInfo> Data = new(StringComparer.OrdinalIgnoreCase)
    {
        ["apple"] = new NutritionInfo { Calories = 52, Protein = 0.3, Fat = 0.2, Carbs = 14.0 },
        ["banana"] = new NutritionInfo { Calories = 89, Protein = 1.1, Fat = 0.3, Carbs = 22.8 },
        ["rice"] = new NutritionInfo { Calories = 130, Protein = 2.7, Fat = 0.3, Carbs = 28.0 },
        ["bread"] = new NutritionInfo { Calories = 265, Protein = 9.0, Fat = 3.2, Carbs = 49.0 },
        ["egg"] = new NutritionInfo { Calories = 155, Protein = 13.0, Fat = 11.0, Carbs = 1.1 },
        ["salad"] = new NutritionInfo { Calories = 33, Protein = 2.0, Fat = 0.4, Carbs = 6.0 }
    };

    public Task<NutritionInfo?> GetNutritionAsync(string foodName, CancellationToken ct)
    {
        _ = ct;
        return Task.FromResult(Data.TryGetValue(foodName, out var nutrition) ? nutrition : null);
    }
}

