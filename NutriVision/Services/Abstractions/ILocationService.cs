namespace NutriVision.Services.Abstractions;

public interface ILocationService
{
    Task<string?> GetCurrentAddressAsync(CancellationToken ct);
}

