using NutriVision.Services.Abstractions;
using System.Net.Http.Json;

namespace NutriVision.Services;

public sealed class LocationService : ILocationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly TimeSpan DeviceFastPathWindow = TimeSpan.FromSeconds(3);

    public LocationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetCurrentAddressAsync(CancellationToken ct)
    {
        var deviceTask = TryGetDeviceAddressAsync(ct);
        var ipTask = TryGetIpAddressAsync(ct);

        // Give device GPS a short head start; fallback to IP early if GPS is slow.
        var fastPath = await Task.WhenAny(deviceTask, Task.Delay(DeviceFastPathWindow, ct));
        if (fastPath == deviceTask)
        {
            var deviceAddress = await AwaitSafe(deviceTask);
            if (!string.IsNullOrWhiteSpace(deviceAddress))
            {
                return deviceAddress;
            }
        }

        var ipAddress = await AwaitSafe(ipTask);
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            return ipAddress;
        }

        // If IP failed, still accept late device result.
        return await AwaitSafe(deviceTask);
    }

    private static async Task<string?> TryGetDeviceAddressAsync(CancellationToken ct)
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                return null;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();
            location ??= await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(6)),
                ct);
            if (location is null)
            {
                return null;
            }

            var places = await Geocoding.GetPlacemarksAsync(location);
            var place = places?.FirstOrDefault();
            if (place is null)
            {
                return $"{location.Latitude:F5}, {location.Longitude:F5}";
            }

            var city = string.IsNullOrWhiteSpace(place.Locality) ? place.SubAdminArea : place.Locality;
            var country = place.CountryName ?? string.Empty;
            var road = place.Thoroughfare ?? string.Empty;
            return $"{city} {road} {country}".Trim();
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> AwaitSafe(Task<string?> task)
    {
        try
        {
            return await task;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> TryGetIpAddressAsync(CancellationToken ct)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(nameof(LocationService));
            client.Timeout = TimeSpan.FromSeconds(6);

            var payload = await client.GetFromJsonAsync<IpWhoResponse>(
                "https://ipwho.is/?fields=success,city,region,country,latitude,longitude",
                ct);

            if (payload is null || payload.Success != true)
            {
                return null;
            }

            var text = $"{payload.City} {payload.Region} {payload.Country}".Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return $"{text} (IP)";
            }

            if (payload.Latitude.HasValue && payload.Longitude.HasValue)
            {
                return $"{payload.Latitude.Value:F5}, {payload.Longitude.Value:F5} (IP)";
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private sealed class IpWhoResponse
    {
        public bool? Success { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
