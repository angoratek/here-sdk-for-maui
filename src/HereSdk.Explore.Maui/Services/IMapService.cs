using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Service for interacting with the map (camera, scene, markers, picking).
/// </summary>
public interface IMapService : IHereSdkService
{
    // Camera control
    Task<GeoCoordinates> GetCameraTargetAsync();
    Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null);
    Task AnimateCameraAsync(CameraAnimation animation);
    double ZoomLevel { get; }
    double Bearing { get; }
    double Tilt { get; }

    // Scene
    Task LoadSceneAsync(MapScheme scheme);
    MapScheme CurrentScheme { get; }

    // Map items
    void AddMapMarker(MapMarker marker);
    void RemoveMapMarker(MapMarker marker);
    void AddMapPolyline(MapPolyline polyline);
    void RemoveMapPolyline(MapPolyline polyline);
    void AddMapPolygon(MapPolygon polygon);
    void RemoveMapPolygon(MapPolygon polygon);
    void AddMapArrow(MapArrow arrow);
    void RemoveMapArrow(MapArrow arrow);

    // 3D markers
    void AddMapMarker3D(MapMarker3D marker);
    void RemoveMapMarker3D(MapMarker3D marker);

    // Picking
    Task<MapPickResult?> PickAsync(Point2D screenPoint);

    // Events
    event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;
    event EventHandler? MapIdle;
}