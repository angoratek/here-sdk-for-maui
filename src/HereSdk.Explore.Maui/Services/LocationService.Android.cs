#pragma warning disable CS1591
#if ANDROID
using Here.Explore.Maui.Models;
using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific LocationService implementation using Microsoft.Maui.Devices.Sensors.Geolocation.
/// Also supports HERE SDK LocationIndicator for on-map visualization.
/// </summary>
public partial class LocationService
{
    public async Task<HereLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.GetLocationAsync(
                new Microsoft.Maui.Devices.Sensors.GeolocationRequest(
                    Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Best,
                    TimeSpan.FromSeconds(10)),
                cancellationToken);

            if (location is null) return null;

            return new HereLocation(
                new GeoCoordinates(location.Latitude, location.Longitude),
                location.Altitude,
                location.Speed,
                location.Course,
                location.IsFromMockProvider ? LocationSource.Passive : LocationSource.Gps);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex}");
            return null;
        }
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (IsListening) return;

        // MAUI's static Geolocation.LocationChanged only fires while a
        // foreground listener is active — subscribing alone is not enough.
        await Microsoft.Maui.Devices.Sensors.Geolocation.StartListeningForegroundAsync(
            new Microsoft.Maui.Devices.Sensors.GeolocationListeningRequest(
                Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(5)));

        Microsoft.Maui.Devices.Sensors.Geolocation.LocationChanged += OnLocationChanged;
        IsListening = true;
    }

    public Task StopListeningAsync()
    {
        if (!IsListening) return Task.CompletedTask;

        Microsoft.Maui.Devices.Sensors.Geolocation.LocationChanged -= OnLocationChanged;
        Microsoft.Maui.Devices.Sensors.Geolocation.StopListeningForeground();
        IsListening = false;
        return Task.CompletedTask;
    }

    private void OnLocationChanged(object? sender, Microsoft.Maui.Devices.Sensors.GeolocationLocationChangedEventArgs e)
    {
        var location = new HereLocation(
            new GeoCoordinates(e.Location.Latitude, e.Location.Longitude),
            e.Location.Altitude,
            e.Location.Speed,
            e.Location.Course,
            e.Location.IsFromMockProvider ? LocationSource.Passive : LocationSource.Gps);
        RaiseLocationChanged(location);
    }
}
#endif
