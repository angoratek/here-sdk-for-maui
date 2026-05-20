#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Models;
using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific LocationService implementation using Microsoft.Maui.Devices.Sensors.Geolocation.
/// HERE SDK native positioning is not yet exposed in the iOS NativeBridge.
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
                LocationSource.Gps);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex}");
            return null;
        }
    }

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (IsListening) return Task.CompletedTask;

        Microsoft.Maui.Devices.Sensors.Geolocation.LocationChanged += OnLocationChanged;
        IsListening = true;
        return Task.CompletedTask;
    }

    public Task StopListeningAsync()
    {
        if (!IsListening) return Task.CompletedTask;

        Microsoft.Maui.Devices.Sensors.Geolocation.LocationChanged -= OnLocationChanged;
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
            LocationSource.Gps);
        RaiseLocationChanged(location);
    }
}
#endif
