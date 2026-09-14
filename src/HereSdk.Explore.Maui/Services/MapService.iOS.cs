#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.PlatformConverters;
using Here.Explore.Maui.Helpers;
using Here.Explore.iOS;
using UIKit;

namespace Here.Explore.Maui.Services;

/// <summary>
/// iOS-specific MapService implementation using NativeBridge wrappers.
/// </summary>
public partial class MapService
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
    private readonly Dictionary<MapMarker3D, HereMapMarker3D> _markers3D = new();
    private readonly Dictionary<MapCircle, HereMapPolygon> _circles = new();
    // NativeBridge doesn't expose clustering, so cluster members are added as
    // plain markers — track which members belong to which shared cluster
    // (reference equality: MapMarkerCluster is a record) so removal is precise.
    private readonly Dictionary<MapMarkerCluster, List<MapMarker>> _clusterMembers =
        new(ReferenceEqualityComparer.Instance);

    public double ZoomLevel => _camera?.State.ZoomLevel ?? 0;
    public double Bearing => _camera?.State.Bearing ?? 0;
    public double Tilt => _camera?.State.Tilt ?? 0;

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
        if (zoomLevel.HasValue)
            _camera.SetTarget(iosCoords, zoomLevel.Value);
        else
            _camera.SetTarget(iosCoords);
    }

    public async Task AnimateCameraAsync(CameraAnimation animation)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var state = _camera.State;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _camera.AnimateLookAt(
            animation.Target.ToiOS(),
            animation.ZoomLevel ?? state.ZoomLevel,
            animation.Bearing ?? state.Bearing,
            animation.Tilt ?? state.Tilt,
            animation.DurationInSeconds,
            (completed, _) => tcs.TrySetResult(completed));
        await tcs.Task;
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
        if (_scene is null) throw new InvalidOperationException("MapService not initialized.");
        var iosCoords = marker.Coordinates.ToiOS();
        var iosMarker = marker.ImagePath is not null
            ? new HereMapMarker(iosCoords.Latitude, iosCoords.Longitude, marker.ImagePath)
            : new HereMapMarker(iosCoords.Latitude, iosCoords.Longitude);
        _scene.AddMapMarker(iosMarker);
        _markers[marker] = iosMarker;
    }

    public void RemoveMapMarker(MapMarker marker)
    {
        if (_scene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers.TryGetValue(marker, out var iosMarker))
        {
            _scene.RemoveMapMarker(iosMarker);
            _markers.Remove(marker);
        }
    }

    public void AddMapPolyline(MapPolyline polyline)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polyline.Vertices.Select(v => v.ToiOS()).ToArray();
        var color = ColorFromHex(polyline.Color);
        // polyline.Cap has no iOS bridge surface — the NativeBridge always
        // renders round caps, so it's ignored here.
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
        var iosPolygon = polygon.StrokeWidthInPixels > 0
            ? new HereMapPolygon(vertices, fillColor,
                ColorFromHex(polygon.StrokeColor), polygon.StrokeWidthInPixels)
            : new HereMapPolygon(vertices, fillColor);
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
        if (_scene is null) throw new InvalidOperationException("MapService not initialized.");
        if (marker.ImagePath is not null)
        {
            var iosMarker = new HereMapMarker3D(
                marker.Coordinates.Latitude, marker.Coordinates.Longitude,
                marker.ImagePath, 32, 32, marker.Scale);
            _scene.AddMapMarker3D(iosMarker);
            _markers3D[marker] = iosMarker;
        }
    }

    public void RemoveMapMarker3D(MapMarker3D marker)
    {
        if (_scene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers3D.TryGetValue(marker, out var iosMarker))
        {
            _scene.RemoveMapMarker3D(iosMarker);
            _markers3D.Remove(marker);
        }
    }

    public async Task<MapPickResult?> PickAsync(Point2D screenPoint)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");

        // NativeBridge picks map items at the point and reports the first picked
        // marker's coordinates — null when nothing was picked.
        var tcs = new TaskCompletionSource<HereGeoCoordinates?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _mapBridgeView.PickFirstMarker(screenPoint.X, screenPoint.Y, (coords, _) => tcs.TrySetResult(coords));
        var picked = await tcs.Task;
        return picked is null
            ? null
            : new MapPickResult(new GeoCoordinates(picked.Latitude, picked.Longitude));
    }

    public void AddMapCircle(MapCircle circle)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = CircleGeometryHelper.GenerateCircleVertices(circle.Center, circle.RadiusInMeters);
        var iosVertices = vertices.Select(v => v.ToiOS()).ToArray();
        var fillColor = ColorFromHex(circle.FillColor);
        // Circles are polygon approximations, so the outline works through
        // the same outline-taking polygon ctor.
        var iosPolygon = circle.StrokeWidthInPixels > 0
            ? new HereMapPolygon(iosVertices, fillColor,
                ColorFromHex(circle.StrokeColor), circle.StrokeWidthInPixels)
            : new HereMapPolygon(iosVertices, fillColor);
        iosPolygon.AddToMapView(_mapBridgeView);
        _circles[circle] = iosPolygon;
    }

    public void RemoveMapCircle(MapCircle circle)
    {
        if (_mapBridgeView is null) throw new InvalidOperationException("MapService not initialized.");
        if (_circles.TryGetValue(circle, out var iosPolygon))
        {
            iosPolygon.RemoveFromMapView(_mapBridgeView);
            _circles.Remove(circle);
        }
    }

    public void ClearAllMapItems()
    {
        if (_scene is null || _mapBridgeView is null) return;

        // Remove all markers
        foreach (var iosMarker in _markers.Values)
            _scene.RemoveMapMarker(iosMarker);
        _markers.Clear();

        // Remove all polylines
        foreach (var iosPolyline in _polylines.Values)
            iosPolyline.RemoveFromMapView(_mapBridgeView);
        _polylines.Clear();

        // Remove all polygons
        foreach (var iosPolygon in _polygons.Values)
            iosPolygon.RemoveFromMapView(_mapBridgeView);
        _polygons.Clear();

        // Remove all arrows
        foreach (var iosArrow in _arrows.Values)
            iosArrow.RemoveFromMapView(_mapBridgeView);
        _arrows.Clear();

        // Remove all 3D markers
        foreach (var iosMarker3D in _markers3D.Values)
            _scene.RemoveMapMarker3D(iosMarker3D);
        _markers3D.Clear();

        // Remove all circles
        foreach (var iosCircle in _circles.Values)
            iosCircle.RemoveFromMapView(_mapBridgeView);
        _circles.Clear();

        // Cluster members were removed with the markers above.
        _clusterMembers.Clear();
    }

    private static UIColor ColorFromHex(uint hex)
    {
        var r = (float)((hex >> 16) & 0xFF) / 255f;
        var g = (float)((hex >> 8) & 0xFF) / 255f;
        var b = (float)(hex & 0xFF) / 255f;
        var a = (float)((hex >> 24) & 0xFF) / 255f;
        return new UIColor(r, g, b, a);
    }

    public void AddMapMarkerCluster(MapMarkerCluster cluster, IEnumerable<MapMarker> markers)
    {
        // iOS NativeBridge doesn't expose marker clustering - markers added individually
        var memberList = new List<MapMarker>();
        foreach (var marker in markers)
        {
            AddMapMarker(marker);
            memberList.Add(marker);
        }
        _clusterMembers[cluster] = memberList;
    }

    public void RemoveMapMarkerCluster(MapMarkerCluster cluster)
    {
        // iOS NativeBridge doesn't expose marker clustering — remove the
        // members that were added individually on AddMapMarkerCluster.
        if (_clusterMembers.TryGetValue(cluster, out var members))
        {
            foreach (var marker in members)
                RemoveMapMarker(marker);
            _clusterMembers.Remove(cluster);
        }
    }

    public void AddLocationIndicator(LocationIndicator indicator)
    {
        // iOS NativeBridge doesn't expose location indicator - use platform location services
    }

    public void UpdateLocationIndicator(GeoCoordinates location, double? bearing = null)
    {
        // iOS NativeBridge doesn't expose location indicator
    }

    public void RemoveLocationIndicator()
    {
        // iOS NativeBridge doesn't expose location indicator
    }
}

internal static class IosMapSchemeConverter
{
    internal static HereMapScheme ToiOSMapScheme(this MapScheme scheme) => scheme switch
    {
        MapScheme.NormalDay => HereMapScheme.NormalDay,
        MapScheme.NormalNight => HereMapScheme.NormalNight,
        MapScheme.HybridDay => HereMapScheme.HybridDay,
        MapScheme.SatelliteDay => HereMapScheme.Satellite,
        MapScheme.TerrainDay => HereMapScheme.RoadNetworkDay, // Closest iOS equivalent
        _ => HereMapScheme.NormalDay,
    };
}
#endif