#if ANDROID
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Com.Here.Sdk.Mapview;
using Com.Here.Sdk.Core;
using Android.Graphics;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific MapService implementation using HERE SDK Android bindings.
/// </summary>
public partial class MapService : IMapService
{
    private MapView? _mapView;
    private MapCamera? _camera;
    private MapScene? _mapScene;
    private Com.Here.Sdk.Gestures.Gestures? _gestures;

    // Track platform map items for removal
    private readonly Dictionary<MapMarker, Com.Here.Sdk.Mapview.MapMarker> _markers = new();
    private readonly Dictionary<MapPolyline, Com.Here.Sdk.Mapview.MapPolyline> _polylines = new();
    private readonly Dictionary<MapPolygon, Com.Here.Sdk.Mapview.MapPolygon> _polygons = new();
    private readonly Dictionary<MapArrow, Com.Here.Sdk.Mapview.MapArrow> _arrows = new();
    private readonly Dictionary<MapMarker3D, Com.Here.Sdk.Mapview.MapMarker3D> _markers3D = new();

    /// <summary>
    /// Initialize the service with an Android MapView.
    /// Called by the handler after the platform view is created.
    /// </summary>
    internal void Initialize(MapView mapView)
    {
        _mapView = mapView;
        _camera = mapView.Camera;
        _mapScene = mapView.MapScene;
        _gestures = mapView.Gestures;
    }

    public override double ZoomLevel => _camera?.State.ZoomLevel ?? 0;
    public override double Bearing => _camera?.State.Orientation.At ?? 0;
    public override double Tilt => _camera?.State.Orientation.Tilt ?? 0;

    public async Task<GeoCoordinates> GetCameraTargetAsync()
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var target = _camera.Target;
        return new GeoCoordinates(target.Latitude, target.Longitude);
    }

    public async Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var androidCoords = new Com.Here.Sdk.Core.GeoCoordinates(target.Latitude, target.Longitude);
        if (zoomLevel.HasValue)
        {
            var update = MapCameraUpdateFactory.LookAt(
                new GeoCoordinatesUpdate(androidCoords.Latitude, androidCoords.Longitude),
                new MapMeasure(MapMeasure.Kind.ZoomLevel, zoomLevel.Value));
            _camera.Update(update);
        }
        else
        {
            var update = MapCameraUpdateFactory.LookAt(
                new GeoCoordinatesUpdate(androidCoords.Latitude, androidCoords.Longitude));
            _camera.Update(update);
        }
    }

    public async Task AnimateCameraAsync(CameraAnimation animation)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var target = new GeoCoordinatesUpdate(animation.Target.Latitude, animation.Target.Longitude);
        var mapMeasure = new MapMeasure(MapMeasure.Kind.ZoomLevel, animation.ZoomLevel ?? _camera.State.ZoomLevel);
        var update = MapCameraUpdateFactory.LookAt(target, mapMeasure);
        var mapAnimation = MapCameraAnimationFactory.CreateAnimation(update, animation.DurationInSeconds);
        _camera.PlayAnimation(mapAnimation);
    }

    public async Task LoadSceneAsync(MapScheme scheme)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var androidScheme = scheme.ToAndroidMapScheme();
        var tcs = new TaskCompletionSource<bool>();
        _mapScene.LoadScene(androidScheme, new SceneLoadCallback(tcs));
        await tcs.Task;
        CurrentScheme = scheme;
    }

    public void AddMapMarker(MapMarker marker)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        var androidCoords = new Com.Here.Sdk.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
        var androidMarker = new Com.Here.Sdk.Mapview.MapMarker(androidCoords, Com.Here.Sdk.Mapview.MapImageFactory.FromResource("marker.png"));
        _mapView.AddMapMarker(androidMarker);
        _markers[marker] = androidMarker;
    }

    public void RemoveMapMarker(MapMarker marker)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers.TryGetValue(marker, out var androidMarker))
        {
            _mapView.RemoveMapMarker(androidMarker);
            _markers.Remove(marker);
        }
    }

    public void AddMapPolyline(MapPolyline polyline)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polyline.Vertices.Select(v => new Com.Here.Sdk.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolyline = new Com.Here.Sdk.Core.GeoPolyline(vertices);
        var lineWidth = new MapMeasureDependentRenderSize(MapMeasureDependentRenderSize.SizeUnit.Pixel, polyline.WidthInPixels);
        var color = new Color((int)(polyline.Color & 0xFFFFFFFF));
        var representation = new MapPolyline.SolidRepresentation(lineWidth, color, LineCap.Round);
        var androidPolyline = new Com.Here.Sdk.Mapview.MapPolyline(geoPolyline, representation);
        _mapView.AddMapPolyline(androidPolyline);
        _polylines[polyline] = androidPolyline;
    }

    public void RemoveMapPolyline(MapPolyline polyline)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polylines.TryGetValue(polyline, out var androidPolyline))
        {
            _mapView.RemoveMapPolyline(androidPolyline);
            _polylines.Remove(polyline);
        }
    }

    public void AddMapPolygon(MapPolygon polygon)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polygon.Vertices.Select(v => new Com.Here.Sdk.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolygon = new Com.Here.Sdk.Core.GeoPolygon(vertices);
        var fillColor = new Color((int)(polygon.FillColor & 0xFFFFFFFF));
        var androidPolygon = new Com.Here.Sdk.Mapview.MapPolygon(geoPolygon, fillColor);
        _mapView.AddMapPolygon(androidPolygon);
        _polygons[polygon] = androidPolygon;
    }

    public void RemoveMapPolygon(MapPolygon polygon)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polygons.TryGetValue(polygon, out var androidPolygon))
        {
            _mapView.RemoveMapPolygon(androidPolygon);
            _polygons.Remove(polygon);
        }
    }

    public void AddMapArrow(MapArrow arrow)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = arrow.Vertices.Select(v => new Com.Here.Sdk.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolyline = new Com.Here.Sdk.Core.GeoPolyline(vertices);
        var color = new Color((int)(arrow.Color & 0xFFFFFFFF));
        var androidArrow = new Com.Here.Sdk.Mapview.MapArrow(geoPolyline, arrow.WidthInPixels, color);
        _mapView.AddMapArrow(androidArrow);
        _arrows[arrow] = androidArrow;
    }

    public void RemoveMapArrow(MapArrow arrow)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_arrows.TryGetValue(arrow, out var androidArrow))
        {
            _mapView.RemoveMapArrow(androidArrow);
            _arrows.Remove(arrow);
        }
    }

    public void AddMapMarker3D(MapMarker3D marker)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        var androidCoords = new Com.Here.Sdk.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
        var androidMarker3D = new Com.Here.Sdk.Mapview.MapMarker3D(androidCoords);
        androidMarker3D.Scale = marker.Scale;
        androidMarker3D.Bearing = marker.Bearing;
        _mapView.AddMapMarker3D(androidMarker3D);
        _markers3D[marker] = androidMarker3D;
    }

    public void RemoveMapMarker3D(MapMarker3D marker)
    {
        if (_mapView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers3D.TryGetValue(marker, out var androidMarker3D))
        {
            _mapView.RemoveMapMarker3D(androidMarker3D);
            _markers3D.Remove(marker);
        }
    }

    public async Task<MapPickResult?> PickAsync(Point2D screenPoint)
    {
        // Will be implemented with full pick result handling
        return null;
    }

    private static MapScheme ToAndroidMapScheme(this MapScheme scheme) => scheme switch
    {
        MapScheme.NormalDay => MapScheme.NormalDay,
        MapScheme.NormalNight => MapScheme.NormalNight,
        MapScheme.HybridDay => MapScheme.HybridDay,
        MapScheme.SatelliteDay => MapScheme.SatelliteDay,
        MapScheme.TerrainDay => MapScheme.TerrainDay,
        _ => MapScheme.NormalDay,
    };
}

/// <summary>
/// Callback for scene loading completion.
/// </summary>
internal class SceneLoadCallback : Java.Lang.Object, MapScene.ISceneLoadCallback
{
    private readonly TaskCompletionSource<bool> _tcs;

    public SceneLoadCallback(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    public void OnLoadScene(MapScene.SceneError? error)
    {
        if (error is null || error.Value == MapScene.SceneError.None)
            _tcs.SetResult(true);
        else
            _tcs.SetException(new Exception($"Scene load error: {error.Value}"));
    }
}
#endif