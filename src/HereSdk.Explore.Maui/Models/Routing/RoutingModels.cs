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
/// Truck vehicle specification applied when
/// <see cref="RoutingOptions.TransportMode"/> is
/// <see cref="SectionTransportMode.Truck"/>. Fields left <c>null</c> fall
/// back to the HERE SDK defaults. Mirrors the SDK's
/// <c>VehicleSpecification.TruckBuilder</c>.
/// </summary>
public record TruckVehicleSpecifications(
    int? GrossWeightInKilograms = null,
    int? HeightInCentimeters = null,
    int? WidthInCentimeters = null,
    int? LengthInCentimeters = null,
    int? AxleCount = null,
    int? TrailerCount = null
);

/// <summary>
/// Options for route calculation.
/// </summary>
public record RoutingOptions(
    OptimizationMode Optimization = OptimizationMode.Fastest,
    SectionTransportMode TransportMode = SectionTransportMode.Car,
    int? MaxAlternatives = null,
    double? DepartureTime = null,
    TruckVehicleSpecifications? Truck = null
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
/// Result of a traffic-along-route calculation.
/// </summary>
public record TrafficOnRouteResult(RoutingError Error, TrafficOnRoute? TrafficOnRoute);

/// <summary>
/// Traffic information along a route, as calculated by
/// <c>GetTrafficOnRouteAsync</c>. Information for the already-traveled
/// portion of the route is omitted.
/// </summary>
public record TrafficOnRoute(
    int LastTraveledSectionIndex,
    int TraveledDistanceOnLastSectionInMeters,
    IReadOnlyList<TrafficOnSection> TrafficSections
);

/// <summary>
/// A route section with real-time traffic data: geometry, traffic spans,
/// and incidents. Mirrors the SDK's <c>TrafficOnSection</c>.
/// </summary>
public record TrafficOnSection(
    IReadOnlyList<GeoCoordinates> Geometry,
    IReadOnlyList<TrafficOnSpan> TrafficSpans,
    IReadOnlyList<TrafficIncidentOnRoute> TrafficIncidents
);

/// <summary>
/// A traffic span along a route section: traffic conditions for a
/// portion of the section geometry. Mirrors the SDK's <c>TrafficOnSpan</c>.
/// <paramref name="GeometryOffset"/> is the index into
/// <see cref="TrafficOnSection.Geometry"/> where this span starts.
/// </summary>
public record TrafficOnSpan(
    double JamFactor,
    double LengthInMeters,
    double BaseSpeedInMetersPerSecond,
    double TrafficSpeedInMetersPerSecond,
    double TrafficDelayInSeconds,
    double DurationInSeconds,
    int GeometryOffset,
    IReadOnlyList<int> IncidentIndices
);

/// <summary>
/// A traffic incident on a specific route.
/// </summary>
public record TrafficIncidentOnRoute(
    string? Id,
    TrafficIncidentType Type,
    TrafficIncidentImpact Impact,
    string? Description
);

/// <summary>
/// Type of waypoint in a route.
/// </summary>
public enum WaypointType
{
    /// <summary>A stop waypoint (the route stops here).</summary>
    Stop,
    /// <summary>A start waypoint.</summary>
    Start,
    /// <summary>A pass-through waypoint (the route passes through without stopping).</summary>
    Through
}

/// <summary>
/// Route calculation error codes.
/// </summary>
public enum RoutingError
{
    /// <summary>No error — route calculated successfully.</summary>
    None,
    /// <summary>Network error during route calculation.</summary>
    NetworkError,
    /// <summary>HTTP error from the routing service.</summary>
    HttpError,
    /// <summary>No route found between the specified waypoints.</summary>
    NoRouteFound,
    /// <summary>Invalid parameters provided.</summary>
    InvalidParameters,
    /// <summary>Insufficient memory to calculate the route.</summary>
    InsufficientMemory,
    /// <summary>Routing engine is not initialized.</summary>
    RoutingNotInitialized,
    /// <summary>Error importing the routing graph.</summary>
    GraphImportError,
    /// <summary>Error searching the routing graph.</summary>
    GraphSearchError,
    /// <summary>No graph data available.</summary>
    NoGraphData,
    /// <summary>No matching map data for the route.</summary>
    NoMatchingMapData,
    /// <summary>No matching route found.</summary>
    NoMatchingRoute,
    /// <summary>No acceptable route found (e.g., all routes violate restrictions).</summary>
    NoAcceptableRoute,
    /// <summary>Route computation was cancelled.</summary>
    RouteComputeCancelled,
    /// <summary>Invalid waypoints provided.</summary>
    WaypointError,
    /// <summary>Transit routing is not allowed.</summary>
    TransitNotAllowed,
    /// <summary>Transit route destination is not reachable.</summary>
    TransitRouteNotReachable,
    /// <summary>Transit route not found.</summary>
    TransitRouteNotFound
}

/// <summary>
/// Optimization mode for route calculation.
/// </summary>
public enum OptimizationMode
{
    /// <summary>Optimize for the fastest route.</summary>
    Fastest,
    /// <summary>Optimize for the shortest distance.</summary>
    Shortest
}

/// <summary>
/// Transport mode for a route section.
/// </summary>
public enum SectionTransportMode
{
    /// <summary>Car transport mode.</summary>
    Car,
    /// <summary>Truck transport mode (respects truck restrictions).</summary>
    Truck,
    /// <summary>Pedestrian transport mode.</summary>
    Pedestrian,
    /// <summary>Bicycle transport mode.</summary>
    Bicycle,
    /// <summary>Scooter/moped transport mode.</summary>
    Scooter,
    /// <summary>Bus transport mode.</summary>
    Bus,
    /// <summary>Taxi transport mode.</summary>
    Taxi,
    /// <summary>Public transit transport mode.</summary>
    Transit
}