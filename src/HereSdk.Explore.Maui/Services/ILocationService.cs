using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for retrieving the device location.
/// Uses Microsoft.Maui.Devices.Sensors.Geolocation as the primary source
/// (HERE SDK positioning is not yet exposed in the iOS NativeBridge).
/// </summary>
public interface ILocationService : IHereSdkService
{
    /// <summary>Gets the current device location, or null if unavailable.</summary>
    Task<HereLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);

    /// <summary>Raised when the location changes (if continuous listening is active).</summary>
    event EventHandler<HereLocation>? LocationChanged;

    /// <summary>Starts continuous location updates.</summary>
    Task StartListeningAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops continuous location updates.</summary>
    Task StopListeningAsync();

    /// <summary>Whether location services are currently listening.</summary>
    bool IsListening { get; }
}
