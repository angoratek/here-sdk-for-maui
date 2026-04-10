namespace Here.Explore.Maui.Models;

/// <summary>
/// Represents a geographic location with coordinates and optional metadata.
/// </summary>
public record Location(
    GeoCoordinates Coordinates,
    double? Altitude = null,
    double? SpeedInMetersPerSecond = null,
    double? BearingInDegrees = null,
    LocationSource Source = LocationSource.Unknown
);

/// <summary>
/// Source of a location fix.
/// </summary>
public enum LocationSource
{
    Unknown,
    Gps,
    Network,
    Passive
}