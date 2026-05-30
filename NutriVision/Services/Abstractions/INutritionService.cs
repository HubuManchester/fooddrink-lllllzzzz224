using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface INutritionService
{
    Task<NutritionInfo?> GetNutritionAsync(string foodName, CancellationToken ct);
}

