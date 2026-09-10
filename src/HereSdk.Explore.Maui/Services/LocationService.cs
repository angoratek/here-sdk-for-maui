using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of LocationService — platform implementations are in LocationService.Android.cs and LocationService.iOS.cs.
/// </summary>
public partial class LocationService : ILocationService
{
    private bool _disposed;

    /// <summary>Whether the location service is actively listening for updates.</summary>
    public bool IsListening { get; protected set; }

    /// <summary>Raised when a new device location is available.</summary>
    public event EventHandler<HereLocation>? LocationChanged;

#if !ANDROID && !IOS
    public Task<HereLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Platform-specific implementation required.");

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Platform-specific implementation required.");

    public Task StopListeningAsync()
        => throw new NotImplementedException("Platform-specific implementation required.");
#endif

    /// <summary>Raises the LocationChanged event with the provided location.</summary>
    protected internal void RaiseLocationChanged(HereLocation location)
        => LocationChanged?.Invoke(this, location);

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Location uses MAUI Geolocation on both platforms — no native
                // SDK resources to release.
            }
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
