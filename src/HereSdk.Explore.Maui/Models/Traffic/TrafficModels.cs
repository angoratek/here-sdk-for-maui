using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Traffic;

/// <summary>
/// Traffic flow information for a road segment.
/// </summary>
public record TrafficFlow(
    double JamFactor,
    double SpeedInMetersPerSecond,
    GeoPolyline Geometry,
    double? FreeFlowSpeedInMetersPerSecond = null,
    double? JamFactorUncertainty = null
);

/// <summary>
/// A traffic incident.
/// </summary>
/// <param name="Id">Stable identifier of the incident from the HERE traffic feed.</param>
/// <param name="Description">Human-readable description (typically a localized road name + cause).</param>
/// <param name="Type">High-level category (accident, congestion, …).</param>
/// <param name="Impact">Severity of the delay (minor, major, closed, …).</param>
/// <param name="Geometry">Optional road polyline covered by the incident.</param>
/// <param name="Location">
/// First coordinate of the incident's polyline (the point on the road where
/// the incident starts). Used by the RefApp to drop a marker on the map.
/// On iOS this is read from <c>TrafficIncident.location.polyline.vertices.first</c>;
/// on Android from <c>TrafficIncident.getLocation().getPolyline().getVertices().get(0)</c>.
/// </param>
/// <param name="StartTime">Optional start time as Unix epoch milliseconds.</param>
/// <param name="EndTime">Optional end time as Unix epoch milliseconds.</param>
/// <param name="RoadClosed">True if the incident reports the road as fully closed.</param>
public record TrafficIncident(
    string Id,
    string Description,
    TrafficIncidentType Type,
    TrafficIncidentImpact Impact,
    GeoPolyline? Geometry = null,
    GeoCoordinates? Location = null,
    long? StartTime = null,
    long? EndTime = null,
    bool? RoadClosed = null
);

/// <summary>
/// Result of a traffic flow query.
/// </summary>
public record TrafficFlowResult(TrafficQueryError Error, IReadOnlyList<TrafficFlow>? Flows);

/// <summary>
/// Result of a traffic incidents query.
/// </summary>
public record TrafficIncidentsResult(TrafficQueryError Error, IReadOnlyList<TrafficIncident>? Incidents);

/// <summary>
/// Options for traffic flow queries.
/// </summary>
public record TrafficFlowQueryOptions();

/// <summary>
/// Options for traffic incidents queries.
/// </summary>
public record TrafficIncidentsQueryOptions();

/// <summary>
/// Options for traffic incident lookup.
/// </summary>
public record TrafficIncidentLookupOptions();

/// <summary>
/// Traffic query error codes.
/// </summary>
public enum TrafficQueryError
{
    /// <summary>No error — query succeeded.</summary>
    None,
    /// <summary>Network error during query.</summary>
    NetworkError,
    /// <summary>HTTP error from the traffic service.</summary>
    HttpError,
    /// <summary>Query completed but returned no results.</summary>
    NoResults,
    /// <summary>The query parameters are invalid.</summary>
    InvalidQuery,
    /// <summary>Traffic engine is not initialized.</summary>
    EngineNotInitialized,
    /// <summary>Insufficient memory for the query.</summary>
    InsufficientMemory
}

/// <summary>
/// Type of traffic incident.
/// </summary>
public enum TrafficIncidentType
{
    /// <summary>Unknown incident type.</summary>
    Unknown,
    /// <summary>Traffic accident.</summary>
    Accident,
    /// <summary>Traffic congestion/jam.</summary>
    Congestion,
    /// <summary>Disabled or broken-down vehicle.</summary>
    DisabledVehicle,
    /// <summary>Lane restriction (reduced lanes).</summary>
    LaneRestriction,
    /// <summary>Road closure.</summary>
    RoadClosure,
    /// <summary>Road hazard (debris, animals, etc.).</summary>
    RoadHazard,
    /// <summary>Road construction.</summary>
    Construction,
    /// <summary>Mass transit disruption.</summary>
    MassTransit,
    /// <summary>Planned event (parade, marathon, etc.).</summary>
    PlannedEvent,
    /// <summary>Weather-related incident.</summary>
    Weather,
    /// <summary>Other incident type.</summary>
    Miscellaneous
}

/// <summary>
/// Impact severity of a traffic incident.
/// </summary>
public enum TrafficIncidentImpact
{
    /// <summary>Unknown impact severity.</summary>
    Unknown,
    /// <summary>Minor impact — slight delays.</summary>
    Minor,
    /// <summary>Moderate impact — noticeable delays.</summary>
    Moderate,
    /// <summary>Major impact — significant delays.</summary>
    Major,
    /// <summary>Road is closed.</summary>
    Closed
}