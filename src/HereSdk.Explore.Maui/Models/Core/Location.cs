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
    /// <summary>Unknown location source.</summary>
    Unknown,
    /// <summary>GPS satellite fix.</summary>
    Gps,
    /// <summary>Network-based location (cell/Wi-Fi).</summary>
    Network,
    /// <summary>Passive provider (uses other apps' fixes).</summary>
    Passive
}