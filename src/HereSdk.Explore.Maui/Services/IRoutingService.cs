using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for route calculation and management.
/// </summary>
public interface IRoutingService : IHereSdkService
{
    Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options);
    Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options);
    Task<TrafficOnRoute> GetTrafficOnRouteAsync(Route route);
}