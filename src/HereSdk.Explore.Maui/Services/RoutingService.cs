using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Shared partial of RoutingService — platform implementations are in RoutingService.Android.cs and RoutingService.iOS.cs.
/// </summary>
public partial class RoutingService : IRoutingService
{
    private bool _disposed;

    public Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options) =>
        throw new NotImplementedException("Platform-specific implementation required.");
    public Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route) =>
        throw new NotImplementedException("Platform-specific implementation required.");

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

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}