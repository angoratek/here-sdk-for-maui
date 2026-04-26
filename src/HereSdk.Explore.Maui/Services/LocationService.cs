using HereLocation = Here.Explore.Maui.Models.Location;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of LocationService — platform implementations are in LocationService.Android.cs and LocationService.iOS.cs.
/// </summary>
public partial class LocationService : ILocationService
{
    private bool _disposed;

    public bool IsListening { get; protected set; }

    public event EventHandler<HereLocation>? LocationChanged;

#if !ANDROID && !IOS
    public Task<HereLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Platform-specific implementation required.");

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Platform-specific implementation required.");

    public Task StopListeningAsync()
        => throw new NotImplementedException("Platform-specific implementation required.");
#endif

    protected internal void RaiseLocationChanged(HereLocation location)
        => LocationChanged?.Invoke(this, location);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Platform implementations will clear native resources
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
