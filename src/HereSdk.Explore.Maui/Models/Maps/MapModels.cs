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
/// <summary>
/// A map marker at geographic coordinates.
/// </summary>
/// <remarks>
/// <see cref="AnchorX"/>/<see cref="AnchorY"/> position the image relative to
/// the coordinates, as percentages of the image size (0–100): 50/50 centers
/// the image on the coordinate, 50/100 puts the bottom-center there (typical
/// pin). Anchoring is applied on Android; the iOS bridge does not expose it.
/// </remarks>
/// <remarks>
/// When <see cref="Glyph"/> (a Material Icons codepoint string, e.g.
/// <c>"\ue56c"</c> for restaurant) and/or <see cref="Color"/> are set,
/// a tinted teardrop pin with that glyph instead of using
/// <see cref="ImagePath"/> — pins become visually distinct per item type
/// (restaurant, incident, origin, …). Glyph pins are bottom-anchored
/// (50/100) unless <see cref="AnchorX"/>/<see cref="AnchorY"/> say otherwise.
/// </remarks>
public record MapMarker(
    GeoCoordinates Coordinates,
    string? ImagePath = null,
    string? Text = null,
    int? AnchorX = null,
    int? AnchorY = null,
    uint? Color = null,
    string? Glyph = null
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
/// Marker clustering configuration. Groups nearby markers into clusters.
/// </summary>
public record MapMarkerCluster(
    double MinZoomLevel = 1.0,
    double MaxZoomLevel = 20.0,
    int MinMarkersPerCluster = 2,
    ClusterStyle? Style = null
);

/// <summary>
/// Styling for cluster counters.
/// </summary>
public record ClusterStyle(
    int FontSize = 14,
    uint TextColor = 0xFFFFFFFF,
    ClusterTextAnchor TextAnchor = ClusterTextAnchor.Center,
    int MaxCountNumber = 99
);

/// <summary>
/// Anchor position for cluster text.
/// </summary>
public enum ClusterTextAnchor
{
    /// <summary>Center-anchored cluster text.</summary>
    Center,
    /// <summary>Left-anchored cluster text.</summary>
    Left,
    /// <summary>Right-anchored cluster text.</summary>
    Right,
    /// <summary>Top-anchored cluster text.</summary>
    Top,
    /// <summary>Bottom-anchored cluster text.</summary>
    Bottom
}

/// <summary>
/// Location indicator (user location puck) configuration.
/// </summary>
public record LocationIndicator(
    GeoCoordinates Location,
    double Bearing = 0.0,
    LocationIndicatorStyle Style = LocationIndicatorStyle.Pedestrian,
    bool IsVisible = true
);

/// <summary>
/// Visual style for location indicator.
/// </summary>
public enum LocationIndicatorStyle
{
    /// <summary>Pedestrian location puck (dot with accuracy ring).</summary>
    Pedestrian,
    /// <summary>Navigation location puck (arrow with heading).</summary>
    Navigation
}

/// <summary>
/// A polyline drawn on the map.
/// </summary>
public record MapPolyline(
    IReadOnlyList<GeoCoordinates> Vertices,
    uint Color = 0xFF0000FF,
    int WidthInPixels = 5,
    LineCap Cap = LineCap.Round
);

/// <summary>
/// A polygon drawn on the map.
/// </summary>
/// <remarks>
/// The outline is rendered only when <see cref="StrokeWidthInPixels"/> is
/// greater than 0; a zero width (the default) keeps the outline disabled.
/// </remarks>
public record MapPolygon(
    IReadOnlyList<GeoCoordinates> Vertices,
    uint FillColor = 0x330000FF,
    uint StrokeColor = 0,
    int StrokeWidthInPixels = 0
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
/// A circle drawn on the map. Implemented internally as a polygon approximation.
/// </summary>
public record MapCircle(
    GeoCoordinates Center,
    double RadiusInMeters,
    uint FillColor = 0x330000FF,
    uint StrokeColor = 0xFF0000FF,
    int StrokeWidthInPixels = 2
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
/// Event args for map tapped events.
/// </summary>
public record MapTappedEventArgs(
    GeoCoordinates Coordinates,
    Point2D ScreenPoint
);

/// <summary>
/// Event args for map double-tapped events.
/// </summary>
public record MapDoubleTappedEventArgs(
    GeoCoordinates Coordinates,
    Point2D ScreenPoint
);

/// <summary>
/// Event args for map long-pressed events.
/// </summary>
public record MapLongPressedEventArgs(
    GeoCoordinates Coordinates,
    Point2D ScreenPoint
);

/// <summary>
/// Event args for map panned (dragged) events.
/// </summary>
public record MapPannedEventArgs(
    GeoCoordinates Coordinates,
    Point2D Delta
);

/// <summary>
/// Event args for map pinch/rotate gesture events.
/// </summary>
public record MapPinchRotatedEventArgs(
    double Scale,
    double RotationInDegrees
);

/// <summary>
/// Event args for map pinched events.
/// </summary>
public record MapPinchedEventArgs(
    double Scale,
    GeoCoordinates Center
);

/// <summary>
/// Map scheme (visual style).
/// </summary>
public enum MapScheme
{
    /// <summary>Normal day — light map with labels.</summary>
    NormalDay,
    /// <summary>Normal night — dark map with labels.</summary>
    NormalNight,
    /// <summary>Hybrid day — satellite imagery with labels.</summary>
    HybridDay,
    /// <summary>Satellite day — satellite imagery without labels.</summary>
    SatelliteDay,
    /// <summary>Terrain day — terrain elevation with labels.</summary>
    TerrainDay
}

/// <summary>
/// Draw order type for map items.
/// </summary>
public enum DrawOrderType
{
    /// <summary>Draw above polygon layers.</summary>
    AbovePolygons,
    /// <summary>Draw above polygons and ADAS layers.</summary>
    AbovePolygonsAndAdas,
    /// <summary>Draw below polygon layers.</summary>
    BelowPolygons
}

/// <summary>
/// Line cap style for polyline endpoints.
/// </summary>
public enum LineCap
{
    /// <summary>Rounded line cap.</summary>
    Round,
    /// <summary>Square line cap extending past endpoint.</summary>
    Square,
    /// <summary>Flat line cap at endpoint.</summary>
    Butt
}

/// <summary>
/// Map content category for visibility filtering.
/// </summary>
public enum MapContentCategory
{
    /// <summary>No specific category.</summary>
    NoCategory,
    /// <summary>Points of interest (restaurants, gas stations, etc.).</summary>
    PoiCategory,
    /// <summary>Traffic incidents.</summary>
    TrafficIncidentCategory,
    /// <summary>Car-specific content.</summary>
    CarCategory,
    /// <summary>Truck-specific content.</summary>
    TruckCategory,
    /// <summary>Pedestrian-specific content.</summary>
    PedestrianCategory
}