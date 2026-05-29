using NutriVision.Services.Abstractions;

namespace NutriVision.Services;

public sealed class LocationService : ILocationService
{
    public async Task<string?> GetCurrentAddressAsync(CancellationToken ct)
    {
        _ = ct;
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
        location ??= await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8)));
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
}

