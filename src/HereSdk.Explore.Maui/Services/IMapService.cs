using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for interacting with the map (camera, scene, markers, picking).
/// </summary>
public interface IMapService : IHereSdkService
{
    /// <summary>Gets the current camera target coordinates.</summary>
    Task<GeoCoordinates> GetCameraTargetAsync();

    /// <summary>Sets the camera target to the specified coordinates.</summary>
    /// <param name="target">The geographic coordinates to center the camera on.</param>
    /// <param name="zoomLevel">Optional zoom level (0–22+). Null keeps current zoom.</param>
    Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null);

    /// <summary>Animates the camera to the specified target with optional zoom, bearing, and tilt.</summary>
    /// <param name="animation">Camera animation parameters including target, duration, and optional overrides.</param>
    Task AnimateCameraAsync(CameraAnimation animation);

    /// <summary>Gets the current camera zoom level.</summary>
    double ZoomLevel { get; }

    /// <summary>Gets the current camera bearing in degrees (0–360, clockwise from north).</summary>
    double Bearing { get; }

    /// <summary>Gets the current camera tilt in degrees (0–90, where 0 is top-down).</summary>
    double Tilt { get; }

    /// <summary>Loads a map scene with the specified visual scheme.</summary>
    /// <param name="scheme">The map scheme to load (e.g., NormalDay, NormalNight).</param>
    Task LoadSceneAsync(MapScheme scheme);

    /// <summary>Gets the currently loaded map scheme.</summary>
    MapScheme CurrentScheme { get; }

    /// <summary>Adds a map marker at the specified coordinates.</summary>
    /// <param name="marker">The map marker to add.</param>
    void AddMapMarker(MapMarker marker);

    /// <summary>Removes a previously added map marker.</summary>
    /// <param name="marker">The map marker to remove.</param>
    void RemoveMapMarker(MapMarker marker);

    /// <summary>Adds a polyline overlay on the map.</summary>
    /// <param name="polyline">The polyline to add.</param>
    void AddMapPolyline(MapPolyline polyline);

    /// <summary>Removes a previously added polyline.</summary>
    /// <param name="polyline">The polyline to remove.</param>
    void RemoveMapPolyline(MapPolyline polyline);

    /// <summary>Adds a polygon overlay on the map.</summary>
    /// <param name="polygon">The polygon to add.</param>
    void AddMapPolygon(MapPolygon polygon);

    /// <summary>Removes a previously added polygon.</summary>
    /// <param name="polygon">The polygon to remove.</param>
    void RemoveMapPolygon(MapPolygon polygon);

    /// <summary>Adds a directional arrow overlay on the map.</summary>
    /// <param name="arrow">The arrow to add.</param>
    void AddMapArrow(MapArrow arrow);

    /// <summary>Removes a previously added arrow.</summary>
    /// <param name="arrow">The arrow to remove.</param>
    void RemoveMapArrow(MapArrow arrow);

    /// <summary>Adds a 3D map marker (billboard or model-based).</summary>
    /// <param name="marker">The 3D marker to add.</param>
    void AddMapMarker3D(MapMarker3D marker);

    /// <summary>Removes a previously added 3D marker.</summary>
    /// <param name="marker">The 3D marker to remove.</param>
    void RemoveMapMarker3D(MapMarker3D marker);

    /// <summary>Adds a circle overlay on the map (approximated as a polygon).</summary>
    /// <param name="circle">The circle to add.</param>
    void AddMapCircle(MapCircle circle);

    /// <summary>Removes a previously added circle.</summary>
    /// <param name="circle">The circle to remove.</param>
    void RemoveMapCircle(MapCircle circle);

    /// <summary>Picks map items at the specified screen coordinate.</summary>
    /// <param name="screenPoint">The screen coordinate to pick at.</param>
    /// <returns>The pick result, or null if nothing was found.</returns>
    Task<MapPickResult?> PickAsync(Point2D screenPoint);

    /// <summary>Raised when the camera position changes (move, zoom, rotate, or tilt).</summary>
    event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;

    /// <summary>Raised when the map becomes idle (all rendering and loading complete).</summary>
    event EventHandler? MapIdle;

    /// <summary>Raised when the user taps on the map.</summary>
    event EventHandler<MapTappedEventArgs>? MapTapped;

    /// <summary>Clears all map items (markers, polylines, polygons, arrows).</summary>
    void ClearAllMapItems();
}