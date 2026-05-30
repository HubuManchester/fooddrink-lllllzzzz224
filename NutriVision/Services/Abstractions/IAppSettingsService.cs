using NutriVision.Models;

namespace NutriVision.Services.Abstractions;

public interface IAppSettingsService
{
    Task<AppSettings> LoadAsync(CancellationToken ct);
    Task SaveAsync(AppSettings settings, CancellationToken ct);
}
