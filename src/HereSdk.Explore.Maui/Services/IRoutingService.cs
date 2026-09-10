using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for route calculation and management.
/// </summary>
public interface IRoutingService : IHereSdkService
{
    /// <summary>Calculates routes between the specified waypoints.</summary>
    /// <param name="waypoints">Ordered list of waypoints (at least 2: start and destination).</param>
    /// <param name="options">Routing options (transport mode, optimization, alternatives).</param>
    /// <returns>The routing result containing calculated routes or an error.</returns>
    Task<RoutingResult> CalculateRouteAsync(IReadOnlyList<Waypoint> waypoints, RoutingOptions options);

    /// <summary>Calculates an isoline (reachability polygon) from a center point.</summary>
    /// <param name="center">The center point for the isoline calculation.</param>
    /// <param name="options">Isoline options (transport mode, range, max points).</param>
    /// <returns>The isoline result containing reachable area polygons or an error.</returns>
    Task<IsolineResult> CalculateIsolineAsync(GeoCoordinates center, IsolineOptions options);

    /// <summary>Gets real-time traffic information along a previously calculated route.</summary>
    /// <param name="route">The route to query traffic for (from <see cref="CalculateRouteAsync"/>).</param>
    /// <returns>Traffic information: spans, incidents and per-section geometry.</returns>
    Task<TrafficOnRouteResult> GetTrafficOnRouteAsync(Route route);
}