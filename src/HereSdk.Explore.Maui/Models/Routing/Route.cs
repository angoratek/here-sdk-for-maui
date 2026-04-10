namespace Here.Explore.Maui.Models.Routing;

/// <summary>
/// A calculated route.
/// </summary>
public record Route(
    string Handle,
    IReadOnlyList<Section> Sections,
    double LengthInMeters,
    long DurationInSeconds
);

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
    long DurationInSeconds
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
);

/// <summary>
/// Maneuver action types.
/// </summary>
public enum ManeuverAction
{
    Depart,
    Arrive,
    Left,
    Right,
    SharpLeft,
    SharpRight,
    SlightLeft,
    SlightRight,
    Straight,
    UTurnLeft,
    UTurnRight,
    LeftRamp,
    RightRamp,
    LeftExit,
    RightExit,
    Roundabout,
    Ferry
}