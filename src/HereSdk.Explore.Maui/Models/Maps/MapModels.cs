using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Models.Maps;

/// <summary>
/// Camera animation parameters.
/// </summary>
public record CameraAnimation(
    GeoCoordinates Target,
    double? ZoomLevel = null,
    double? Bearing = null,
    double? Tilt = null,
    double DurationInSeconds = 1.0
);

/// <summary>
/// A map marker at geographic coordinates.
/// </summary>
public record MapMarker(
    GeoCoordinates Coordinates,
    string? ImagePath = null,
    string? Text = null,
    int? AnchorX = null,
    int? AnchorY = null
);

/// <summary>
/// A 3D map marker (billboard or model-based).
/// </summary>
public record MapMarker3D(
    GeoCoordinates Coordinates,
    string? ImagePath = null,
    double Scale = 1.0,
    double Bearing = 0.0,
    double Tilt = 0.0
);

/// <summary>
/// A polyline drawn on the map.
/// </summary>
public record MapPolyline(
    IReadOnlyList<GeoCoordinates> Vertices,
    uint Color = 0xFF0000FF,
    int WidthInPixels = 5
);

/// <summary>
/// A polygon drawn on the map.
/// </summary>
public record MapPolygon(
    IReadOnlyList<GeoCoordinates> Vertices,
    uint FillColor = 0x330000FF
);

/// <summary>
/// An arrow drawn on the map (direction indicator).
/// </summary>
public record MapArrow(
    IReadOnlyList<GeoCoordinates> Vertices,
    uint Color = 0xFF0000FF,
    int WidthInPixels = 5
);

/// <summary>
/// Result of picking map items at a screen coordinate.
/// </summary>
public record MapPickResult(
    GeoCoordinates Coordinates,
    object? PlatformResult = null
);

/// <summary>
/// Event args for camera state changes.
/// </summary>
public record CameraStateChangedEventArgs(
    GeoCoordinates Target,
    double ZoomLevel,
    double Bearing,
    double Tilt
);

/// <summary>
/// Map scheme (visual style).
/// </summary>
public enum MapScheme
{
    NormalDay,
    NormalNight,
    HybridDay,
    SatelliteDay,
    TerrainDay
}

/// <summary>
/// Draw order type for map items.
/// </summary>
public enum DrawOrderType
{
    AbovePolygons,
    AbovePolygonsAndAdas,
    BelowPolygons
}

/// <summary>
/// Line cap style for polyline endpoints.
/// </summary>
public enum LineCap
{
    Round,
    Square,
    Butt
}

/// <summary>
/// Map content category for visibility filtering.
/// </summary>
public enum MapContentCategory
{
    NoCategory,
   poiCategory,
    TrafficIncidentCategory,
    CarCategory,
    TruckCategory,
    PedestrianCategory
}