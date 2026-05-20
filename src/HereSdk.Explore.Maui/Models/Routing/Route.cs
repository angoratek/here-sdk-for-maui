using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Routing;

/// <summary>
/// A calculated route.
/// </summary>
public record Route(
    string Handle,
    IReadOnlyList<Section> Sections,
    double LengthInMeters,
    long DurationInSeconds
)
{
    /// <summary>Human-readable route duration (e.g., "1h 2m", "45s").</summary>
    public string DurationText => DurationInSeconds switch
    {
        >= 3600 => $"{DurationInSeconds / 3600}h {DurationInSeconds % 3600 / 60}m",
        >= 60 => $"{DurationInSeconds / 60}m {DurationInSeconds % 60}s",
        _ => $"{DurationInSeconds}s"
    };
}

/// <summary>
/// A section of a route between two waypoints.
/// </summary>
public record Section(
    int SectionIndex,
    GeoCoordinates Departure,
    GeoCoordinates Arrival,
    IReadOnlyList<Maneuver> Maneuvers,
    SectionTransportMode TransportMode,
    double LengthInMeters,
    long DurationInSeconds,
    IReadOnlyList<GeoCoordinates>? Geometry = null
);

/// <summary>
/// A maneuver instruction along a route.
/// </summary>
public record Maneuver(
    GeoCoordinates Coordinates,
    ManeuverAction Action,
    string? Instruction = null,
    double? BearingBefore = null,
    double? BearingAfter = null,
    string? NextRoadName = null,
    string? NextRoadNumber = null,
    double? LengthInMeters = null,
    long? DurationInSeconds = null
)
{
    /// <summary>Human-readable maneuver distance (e.g., "250 m").</summary>
    public string DistanceText => LengthInMeters.HasValue ? $"{LengthInMeters.Value:F0} m" : "";
}

/// <summary>
/// Maneuver action types.
/// </summary>
public enum ManeuverAction
{
    /// <summary>Depart from the start point.</summary>
    Depart,
    /// <summary>Arrive at the destination.</summary>
    Arrive,
    /// <summary>Turn left.</summary>
    Left,
    /// <summary>Turn right.</summary>
    Right,
    /// <summary>Make a sharp left turn.</summary>
    SharpLeft,
    /// <summary>Make a sharp right turn.</summary>
    SharpRight,
    /// <summary>Make a slight left turn.</summary>
    SlightLeft,
    /// <summary>Make a slight right turn.</summary>
    SlightRight,
    /// <summary>Continue straight.</summary>
    Straight,
    /// <summary>Make a U-turn to the left.</summary>
    UTurnLeft,
    /// <summary>Make a U-turn to the right.</summary>
    UTurnRight,
    /// <summary>Take the left ramp.</summary>
    LeftRamp,
    /// <summary>Take the right ramp.</summary>
    RightRamp,
    /// <summary>Take the left exit.</summary>
    LeftExit,
    /// <summary>Take the right exit.</summary>
    RightExit,
    /// <summary>Enter a roundabout.</summary>
    Roundabout,
    /// <summary>Continue on the current road.</summary>
    ContinueOn
}