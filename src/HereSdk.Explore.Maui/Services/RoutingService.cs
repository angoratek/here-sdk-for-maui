using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of RoutingService — platform implementations are in RoutingService.Android.cs and RoutingService.iOS.cs.
/// </summary>
public partial class RoutingService : IRoutingService
{
    private bool _disposed;

#if !ANDROID && !IOS
    // Non-device stub: tracks initialization in-memory so unit/integration
    // tests can assert that DI factories call Initialize() correctly.
    private bool _stubInitialized;
    internal void Initialize() => _stubInitialized = true;
#endif

    /// <summary>
    /// Gets whether the service's native routing engine has been created.
    /// False until <see cref="Initialize"/> runs. Used by integration tests
    /// to confirm the DI factory wired initialization correctly.
    /// </summary>
    public bool IsInitialized =>
#if ANDROID || IOS
        _engine is not null;
#else
        _stubInitialized;
#endif

#if !ANDROID && !IOS
    // Non-platform stub implementations for unit-test context
    public Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route) =>
        throw new NotImplementedException("Platform-specific implementation required.");
#endif

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Platform implementations will dispose native engine
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