#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.PlatformConverters;
using UIKit;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific MapService implementation using NativeBridge wrappers.
/// </summary>
public partial class MapService : IMapService
{
    private HereMapCamera? _camera;
    private HereMapScene? _scene;
    private HereGestures? _gestures;
    private HereMapBridgeView? _mapBridgeView;

    // Track platform map items for removal
    private readonly Dictionary<MapMarker, HereMapMarker> _markers = new();
    private readonly Dictionary<MapPolyline, HereMapPolyline> _polylines = new();
    private readonly Dictionary<MapPolygon, HereMapPolygon> _polygons = new();
    private readonly Dictionary<MapArrow, HereMapArrow> _arrows = new();

    public override double ZoomLevel => _camera?.State.ZoomLevel ?? 0;
    public override double Bearing => _camera?.State.Bearing ?? 0;
    public override double Tilt => _camera?.State.Tilt ?? 0;

    /// <summary>
    /// Initialize the service with iOS MapView components.
    /// Called by the handler after the platform view is created.
    /// </summary>
    internal void Initialize(HereMapCamera camera, HereMapScene scene, HereGestures gestures, HereMapBridgeView mapBridgeView)
    {
        _camera = camera;
        _scene = scene;
        _gestures = gestures;
        _mapBridgeView = mapBridgeView;
    }

    public async Task<GeoCoordinates> GetCameraTargetAsync()
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var state = _camera.State;
        return new GeoCoordinates(state.TargetLatitude, state.TargetLongitude);
    }

    public async Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var iosCoords = target.ToiOS();
        _camera.SetTarget(iosCoords, zoomLevel ?? -1);
    }

    public async Task AnimateCameraAsync(CameraAnimation animation)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var iosCoords = animation.Target.ToiOS();
        _camera.SetTarget(iosCoords, animation.ZoomLevel ?? -1);
    }

    public async Task LoadSceneAsync(MapScheme scheme)
    {
        if (_scene is null) throw new InvalidOperationException("MapService not initialized.");
        var iosScheme = scheme.ToiOSMapScheme();
        _scene.LoadScene(iosScheme);
        CurrentScheme = scheme;
        await Task.CompletedTask;
    }

    public void AddMapMarker(MapMarker marker)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var iosCoords = marker.Coordinates.ToiOS();
        var iosMarker = new HereMapMarker(iosCoords);
        _mapBridgeView.AddMapMarker(iosMarker);
        _markers[marker] = iosMarker;
    }

    public void RemoveMapMarker(MapMarker marker)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers.TryGetValue(marker, out var iosMarker))
        {
            _mapBridgeView.RemoveMapMarker(iosMarker);
            _markers.Remove(marker);
        }
    }

    public void AddMapPolyline(MapPolyline polyline)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polyline.Vertices.Select(v => v.ToiOS()).ToArray();
        var color = ColorFromHex(polyline.Color);
        var iosPolyline = new HereMapPolyline(vertices, color, polyline.WidthInPixels);
        iosPolyline.AddToMapView(_mapBridgeView);
        _polylines[polyline] = iosPolyline;
    }

    public void RemoveMapPolyline(MapPolyline polyline)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polylines.TryGetValue(polyline, out var iosPolyline))
        {
            iosPolyline.RemoveFromMapView(_mapBridgeView);
            _polylines.Remove(polyline);
        }
    }

    public void AddMapPolygon(MapPolygon polygon)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polygon.Vertices.Select(v => v.ToiOS()).ToArray();
        var fillColor = ColorFromHex(polygon.FillColor);
        var iosPolygon = new HereMapPolygon(vertices, fillColor);
        iosPolygon.AddToMapView(_mapBridgeView);
        _polygons[polygon] = iosPolygon;
    }

    public void RemoveMapPolygon(MapPolygon polygon)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polygons.TryGetValue(polygon, out var iosPolygon))
        {
            iosPolygon.RemoveFromMapView(_mapBridgeView);
            _polygons.Remove(polygon);
        }
    }

    public void AddMapArrow(MapArrow arrow)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = arrow.Vertices.Select(v => v.ToiOS()).ToArray();
        var color = ColorFromHex(arrow.Color);
        var iosArrow = new HereMapArrow(vertices, arrow.WidthInPixels, color);
        iosArrow.AddToMapView(_mapBridgeView);
        _arrows[arrow] = iosArrow;
    }

    public void RemoveMapArrow(MapArrow arrow)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_arrows.TryGetValue(arrow, out var iosArrow))
        {
            iosArrow.RemoveFromMapView(_mapBridgeView);
            _arrows.Remove(arrow);
        }
    }

    public void AddMapMarker3D(MapMarker3D marker)
    {
        // MapMarker3D support on iOS requires NativeBridge wrapper
        // Will be expanded with HereMapMarker3D NativeBridge class
        throw new NotImplementedException("MapMarker3D support pending NativeBridge wrapper.");
    }

    public void RemoveMapMarker3D(MapMarker3D marker)
    {
        throw new NotImplementedException("MapMarker3D support pending NativeBridge wrapper.");
    }

    public async Task<MapPickResult?> PickAsync(Point2D screenPoint) => null;

    private static UIColor ColorFromHex(uint hex)
    {
        var r = (float)((hex >> 16) & 0xFF) / 255f;
        var g = (float)((hex >> 8) & 0xFF) / 255f;
        var b = (float)(hex & 0xFF) / 255f;
        var a = (float)((hex >> 24) & 0xFF) / 255f;
        return new UIColor(r, g, b, a);
    }
}

internal static class IosMapSchemeConverter
{
    internal static HereMapScheme ToiOSMapScheme(this MapScheme scheme) => scheme switch
    {
        MapScheme.NormalDay => HereMapScheme.NormalDay,
        MapScheme.NormalNight => HereMapScheme.NormalNight,
        MapScheme.HybridDay => HereMapScheme.HybridDay,
        MapScheme.SatelliteDay => HereMapScheme.SatelliteDay,
        MapScheme.TerrainDay => HereMapScheme.NormalDay, // No direct TerrainDay on iOS
        _ => HereMapScheme.NormalDay,
    };
}
#endif