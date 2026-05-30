using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface IAppAppearanceService
{
    void Apply(AppSettings settings);
}
