using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Models.Routing;

/// <summary>
/// A waypoint used in route calculation.
/// </summary>
public record Waypoint(
    GeoCoordinates Coordinates,
    WaypointType Type = WaypointType.Stop,
    string? Name = null,
    string? Hint = null
);

/// <summary>
/// Result of a route calculation.
/// </summary>
public record RoutingResult(RoutingError Error, IReadOnlyList<Route>? Routes);

/// <summary>
/// Result of an isoline calculation.
/// </summary>
public record IsolineResult(RoutingError Error, IReadOnlyList<Isoline>? Isolines);

/// <summary>
/// An isoline (reachability polygon) from a center point.
/// </summary>
public record Isoline(IReadOnlyList<GeoCoordinates> Polygon, double RangeInMeters);

/// <summary>
/// Options for route calculation.
/// </summary>
public record RoutingOptions(
    OptimizationMode Optimization = OptimizationMode.Fastest,
    SectionTransportMode TransportMode = SectionTransportMode.Car,
    int? MaxAlternatives = null,
    double? DepartureTime = null
);

/// <summary>
/// Options for isoline calculation.
/// </summary>
public record IsolineOptions(
    SectionTransportMode TransportMode = SectionTransportMode.Car,
    double RangeInMeters = 10000,
    int? MaxPoints = null
);

/// <summary>
/// Traffic information along a route.
/// </summary>
public record TrafficOnRoute(
    string RouteHandle,
    IReadOnlyList<TrafficIncidentOnRoute>? Incidents = null,
    double? DelayInSeconds = null
);

/// <summary>
/// A traffic incident on a specific route.
/// </summary>
public record TrafficIncidentOnRoute(
    string Id,
    string Description,
    TrafficIncidentType Type,
    TrafficIncidentImpact Impact,
    int? AffectedSectionIndex = null
);

/// <summary>
/// Type of waypoint in a route.
/// </summary>
public enum WaypointType
{
    Stop,
    Start,
    Through
}

/// <summary>
/// Route calculation error codes.
/// </summary>
public enum RoutingError
{
    None,
    NetworkError,
    HttpError,
    NoRouteFound,
    InvalidParameters,
    InsufficientMemory,
    RoutingNotInitialized,
    GraphImportError,
    GraphSearchError,
    NoGraphData,
    NoMatchingMapData,
    NoMatchingRoute,
    NoAcceptableRoute,
    RouteComputeCancelled,
    WaypointError,
    TransitNotAllowed,
    TransitRouteNotReachable,
    TransitRouteNotFound
}

/// <summary>
/// Optimization mode for route calculation.
/// </summary>
public enum OptimizationMode
{
    Fastest,
    Shortest
}

/// <summary>
/// Transport mode for a route section.
/// </summary>
public enum SectionTransportMode
{
    Car,
    Truck,
    Pedestrian,
    Bicycle,
    Scooter,
    Bus,
    Taxi,
    Transit
}