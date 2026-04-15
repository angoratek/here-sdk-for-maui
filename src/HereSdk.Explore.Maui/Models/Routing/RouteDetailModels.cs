using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Routing;

/// <summary>
/// Handle to a calculated route (can be used to refresh or get traffic).
/// </summary>
public record RouteHandle(string Id);

/// <summary>
/// A span within a route section (smallest unit between two geometry points).
/// </summary>
public record Span(
    GeoCoordinates Departure,
    GeoCoordinates Arrival,
    int? LengthInMeters = null,
    long? DurationInSeconds = null,
    int? FunctionalRoadClass = null
);

/// <summary>
/// Toll information for a route or section.
/// </summary>
public record Toll(
    decimal? TotalPrice = null,
    string? Currency = null,
    string? CountryCode = null
);

/// <summary>
/// A notice/warning for a route section.
/// </summary>
public record SectionNotice(
    SectionNoticeCode Code,
    NoticeSeverity Severity,
    string? Text = null
);

/// <summary>
/// A named place along a route (departure/arrival station, parking, etc.).
/// </summary>
public record RoutePlace(
    GeoCoordinates Coordinates,
    string? Name = null,
    RoutePlaceType Type = RoutePlaceType.Unknown
);

/// <summary>
/// Road signpost information at a maneuver point.
/// </summary>
public record Signpost(
    string? RoadNumber = null,
    string? RoadName = null,
    string? ExitNumber = null,
    string? Toward = null
);

/// <summary>
/// Section notice codes indicating road features or restrictions.
/// </summary>
public enum SectionNoticeCode
{
    /// <summary>Unknown notice code.</summary>
    Unknown,
    /// <summary>Toll road section.</summary>
    TollRoad,
    /// <summary>Restricted area entry.</summary>
    RestrictedArea,
    /// <summary>Low emission zone.</summary>
    LowEmissionZone,
    /// <summary>Hazardous material restriction.</summary>
    HazardousMaterialRestriction,
    /// <summary>Weight restriction.</summary>
    WeightRestriction,
    /// <summary>Height restriction.</summary>
    HeightRestriction,
    /// <summary>Railway crossing.</summary>
    RailwayCrossing,
    /// <summary>Unpaved road.</summary>
    UnpavedRoad,
    /// <summary>Ferry crossing.</summary>
    FerryCrossing,
    /// <summary>Car shuttle train.</summary>
    CarShuttleTrain,
    /// <summary>Country border crossing.</summary>
    CountryBorder
}

/// <summary>
/// Severity of a route section notice.
/// </summary>
public enum NoticeSeverity
{
    /// <summary>Unknown severity.</summary>
    Unknown,
    /// <summary>Informational notice.</summary>
    Info,
    /// <summary>Warning notice.</summary>
    Warning
}

/// <summary>
/// Type of place along a route.
/// </summary>
public enum RoutePlaceType
{
    /// <summary>Unknown place type.</summary>
    Unknown,
    /// <summary>Train/transit station.</summary>
    Station,
    /// <summary>Parking facility.</summary>
    Parking,
    /// <summary>Rest area.</summary>
    RestArea,
    /// <summary>Charging station.</summary>
    ChargingStation,
    /// <summary>Fuel station.</summary>
    FuelStation
}