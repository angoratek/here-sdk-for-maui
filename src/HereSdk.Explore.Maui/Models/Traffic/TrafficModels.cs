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
public record TrafficIncident(
    string Id,
    string Description,
    TrafficIncidentType Type,
    TrafficIncidentImpact Impact,
    GeoPolyline? Geometry = null,
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
    None,
    NetworkError,
    HttpError,
    NoResults,
    InvalidQuery,
    EngineNotInitialized,
    InsufficientMemory
}

/// <summary>
/// Type of traffic incident.
/// </summary>
public enum TrafficIncidentType
{
    Unknown,
    Accident,
    Congestion,
    DisabledVehicle,
    LaneRestriction,
    RoadClosure,
    RoadHazard,
    Construction,
    MassTransit,
    PlannedEvent,
    Weather,
    Miscellaneous
}

/// <summary>
/// Impact severity of a traffic incident.
/// </summary>
public enum TrafficIncidentImpact
{
    Unknown,
    Minor,
    Moderate,
    Major,
    Closed
}