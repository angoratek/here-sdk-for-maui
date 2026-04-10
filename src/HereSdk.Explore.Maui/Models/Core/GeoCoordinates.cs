namespace Here.Explore.Maui.Models;

/// <summary>
/// Geographic coordinates (latitude/longitude).
/// </summary>
public record GeoCoordinates(double Latitude, double Longitude)
{
    /// <summary>
    /// Whether the coordinates are within valid ranges.
    /// Latitude: -90 to 90, Longitude: -180 to 180.
    /// </summary>
    public bool IsValid => Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
}

/// <summary>
/// Geographic bounding box defined by southwest and northeast corners.
/// </summary>
public record GeoBox(GeoCoordinates SouthWest, GeoCoordinates NorthEast);

/// <summary>
/// Geographic circle defined by center and radius.
/// </summary>
public record GeoCircle(GeoCoordinates Center, double RadiusInMeters);

/// <summary>
/// Geographic corridor defined by a polyline and radius.
/// </summary>
public record GeoCorridor(GeoPolyline Polyline, double RadiusInMeters);

/// <summary>
/// Geographic orientation (bearing and tilt).
/// </summary>
public record GeoOrientation(double Bearing, double Tilt);

/// <summary>
/// Geographic polygon defined by vertices.
/// </summary>
public record GeoPolygon(IReadOnlyList<GeoCoordinates> Vertices);

/// <summary>
/// Geographic polyline defined by vertices.
/// </summary>
public record GeoPolyline(IReadOnlyList<GeoCoordinates> Vertices);

/// <summary>
/// 2D point with X and Y coordinates.
/// </summary>
public record Point2D(double X, double Y);

/// <summary>
/// 2D size with width and height.
/// </summary>
public record Size2D(double Width, double Height);

/// <summary>
/// 2D rectangle defined by origin and size.
/// </summary>
public record Rectangle2D(Point2D Origin, Size2D Size);