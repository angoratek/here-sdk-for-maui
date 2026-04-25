#if ANDROID
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Services;

/// <summary>
/// Android-specific MapService implementation using HERE SDK Android bindings.
/// Key API differences from assumed signatures:
/// - Map item add/remove methods are on MapScene (not MapView)
/// - Method names use lowercase 'd': AddMapMarker3d, RemoveMapMarker3d
/// - Camera: LookAt() directly (not Update()), GetState() for state
/// - GeoCoordinatesUpdate takes Java.Lang.Double (not double)
/// - Core.Color uses (float green, float alpha, float red, float blue)
/// - MapPolyline.SolidRepresentation is MapPolylineSolidRepresentation
/// - MapScheme is a Java enum with static properties
/// - SDKNativeEngine: MakeSharedInstance(context, options), SharedInstance property
/// - SDKOptions takes AuthenticationMode in constructor
/// </summary>
public partial class MapService
{
    private Here.Explore.Maps.MapView? _mapView;
    private Here.Explore.Maps.MapCamera? _camera;
    private Here.Explore.Maps.MapScene? _mapScene;

    // Track platform map items for removal
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapMarker, Here.Explore.Maps.MapMarker> _markers = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapPolyline, Here.Explore.Maps.MapPolyline> _polylines = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapPolygon, Here.Explore.Maps.MapPolygon> _polygons = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapArrow, Here.Explore.Maps.MapArrow> _arrows = new();
    private readonly Dictionary<Here.Explore.Maui.Models.Maps.MapMarker3D, Here.Explore.Maps.MapMarker3D> _markers3D = new();

    internal void Initialize(Here.Explore.Maps.MapView mapView)
    {
        _mapView = mapView;
        _camera = mapView.Camera;
        _mapScene = mapView.MapScene;
    }

    public double ZoomLevel => _camera?.GetState().ZoomLevel ?? 0;
    public double Bearing => _camera?.GetState().OrientationAtTarget.Bearing ?? 0;
    public double Tilt => _camera?.GetState().OrientationAtTarget.Tilt ?? 0;

    public Task<GeoCoordinates> GetCameraTargetAsync()
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var state = _camera.GetState();
        return Task.FromResult(new GeoCoordinates(state.TargetCoordinates.Latitude, state.TargetCoordinates.Longitude));
    }

    public Task SetCameraTargetAsync(GeoCoordinates target, double? zoomLevel = null)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var geoUpdate = new Here.Explore.Core.GeoCoordinatesUpdate(
            (Java.Lang.Double?)target.Longitude, (Java.Lang.Double?)target.Latitude);
        if (zoomLevel.HasValue)
        {
            _camera.LookAt(new Here.Explore.Core.GeoCoordinates(target.Latitude, target.Longitude),
                new Here.Explore.Core.GeoOrientationUpdate(new Here.Explore.Core.GeoOrientation(0, 0)),
                new Here.Explore.Maps.MapMeasure(Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!, zoomLevel.Value));
        }
        else
        {
            _camera.LookAt(new Here.Explore.Core.GeoCoordinates(target.Latitude, target.Longitude));
        }
        return Task.CompletedTask;
    }

    public Task AnimateCameraAsync(Here.Explore.Maui.Models.Maps.CameraAnimation animation)
    {
        if (_camera is null) throw new InvalidOperationException("MapService not initialized.");
        var geoUpdate = new Here.Explore.Core.GeoCoordinatesUpdate(
            (Java.Lang.Double?)animation.Target.Longitude, (Java.Lang.Double?)animation.Target.Latitude);
        var orientation = new Here.Explore.Core.GeoOrientationUpdate(
            new Here.Explore.Core.GeoOrientation(animation.Bearing ?? 0, animation.Tilt ?? 0));
        var mapMeasure = new Here.Explore.Maps.MapMeasure(
            Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!, animation.ZoomLevel ?? _camera.GetState().ZoomLevel);
        var cameraUpdate = Here.Explore.Maps.MapCameraUpdateFactory.LookAt(geoUpdate, orientation, mapMeasure);
        var duration = Com.Here.Time.HereDuration.OfSeconds((long)animation.DurationInSeconds);
        var mapAnimation = Here.Explore.Maps.MapCameraAnimationFactory.CreateAnimation(
            cameraUpdate, duration!, new Here.Explore.Animation.Easing(Here.Explore.Animation.EasingFunction.Linear!));
        // PlayAnimation is on MapView — but let's use the update directly
        // The camera LookAt with update is synchronous; animation requires MapView support
        _camera.LookAt(
            new Here.Explore.Core.GeoCoordinates(animation.Target.Latitude, animation.Target.Longitude),
            orientation,
            mapMeasure);
        return Task.CompletedTask;
    }

    public async Task LoadSceneAsync(Here.Explore.Maui.Models.Maps.MapScheme scheme)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var androidScheme = ToAndroidMapScheme(scheme);
        var tcs = new TaskCompletionSource<bool>();
        _mapScene.LoadScene(androidScheme, new SceneLoadCallback(tcs));
        await tcs.Task;
        CurrentScheme = scheme;
    }

    public void AddMapMarker(Here.Explore.Maui.Models.Maps.MapMarker marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var androidCoords = new Here.Explore.Core.GeoCoordinates(marker.Coordinates.Latitude, marker.Coordinates.Longitude);
        // MapImage requires an Android drawable resource — attempt to load custom marker, fallback to system icon
        Here.Explore.Maps.MapImage? mapImage = null;
        try
        {
            var resId = Platform.AppContext.Resources?.GetIdentifier("marker", "drawable", Platform.AppContext.PackageName) ?? 0;
            if (resId != 0)
                mapImage = Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, resId);
        }
        catch { /* fallback below */ }

        // Use a simple 1x1 pixel fallback if no resource found
        mapImage ??= Here.Explore.Maps.MapImageFactory.FromResource(Platform.AppContext.Resources, global::Android.Resource.Drawable.IcMenuCompass);
        var androidMarker = new Here.Explore.Maps.MapMarker(androidCoords, mapImage!);
        _mapScene.AddMapMarker(androidMarker);
        _markers[marker] = androidMarker;
    }

    public void RemoveMapMarker(Here.Explore.Maui.Models.Maps.MapMarker marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers.TryGetValue(marker, out var androidMarker))
        {
            _mapScene.RemoveMapMarker(androidMarker);
            _markers.Remove(marker);
        }
    }

    public void AddMapPolyline(Here.Explore.Maui.Models.Maps.MapPolyline polyline)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polyline.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolyline = new Here.Explore.Core.GeoPolyline(vertices);
        var lineWidth = new Here.Explore.Maps.MapMeasureDependentRenderSize(
            Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!,
            Here.Explore.Maps.RenderSize.Unit.DensityIndependentPixels!,
            new System.Collections.Generic.Dictionary<Java.Lang.Double, Java.Lang.Double> { [(Java.Lang.Double)polyline.WidthInPixels] = (Java.Lang.Double)polyline.WidthInPixels });
        var color = ToCoreColor(polyline.Color);
        var representation = new Here.Explore.Maps.MapPolyline.MapPolylineSolidRepresentation(lineWidth, color, Here.Explore.Maps.LineCap.Round!);
        var androidPolyline = new Here.Explore.Maps.MapPolyline(geoPolyline, representation);
        _mapScene.AddMapPolyline(androidPolyline);
        _polylines[polyline] = androidPolyline;
    }

    public void RemoveMapPolyline(Here.Explore.Maui.Models.Maps.MapPolyline polyline)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polylines.TryGetValue(polyline, out var androidPolyline))
        {
            _mapScene.RemoveMapPolyline(androidPolyline);
            _polylines.Remove(polyline);
        }
    }

    public void AddMapPolygon(Here.Explore.Maui.Models.Maps.MapPolygon polygon)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = polygon.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolygon = new Here.Explore.Core.GeoPolygon(vertices);
        var fillColor = ToCoreColor(polygon.FillColor);
        var androidPolygon = new Here.Explore.Maps.MapPolygon(geoPolygon, fillColor);
        _mapScene.AddMapPolygon(androidPolygon);
        _polygons[polygon] = androidPolygon;
    }

    public void RemoveMapPolygon(Here.Explore.Maui.Models.Maps.MapPolygon polygon)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_polygons.TryGetValue(polygon, out var androidPolygon))
        {
            _mapScene.RemoveMapPolygon(androidPolygon);
            _polygons.Remove(polygon);
        }
    }

    public void AddMapArrow(Here.Explore.Maui.Models.Maps.MapArrow arrow)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        var vertices = arrow.Vertices.Select(v => new Here.Explore.Core.GeoCoordinates(v.Latitude, v.Longitude)).ToList();
        var geoPolyline = new Here.Explore.Core.GeoPolyline(vertices);
        var color = ToCoreColor(arrow.Color);
        var androidArrow = new Here.Explore.Maps.MapArrow(geoPolyline, arrow.WidthInPixels, color);
        _mapScene.AddMapArrow(androidArrow);
        _arrows[arrow] = androidArrow;
    }

    public void RemoveMapArrow(Here.Explore.Maui.Models.Maps.MapArrow arrow)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_arrows.TryGetValue(arrow, out var androidArrow))
        {
            _mapScene.RemoveMapArrow(androidArrow);
            _arrows.Remove(arrow);
        }
    }

    public void AddMapMarker3D(Here.Explore.Maui.Models.Maps.MapMarker3D marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        // MapMarker3D requires a MapImage or MapMarker3DModel
        // For now, stub until we have proper 3D model support
        throw new NotImplementedException("MapMarker3D requires MapImage or MapMarker3DModel.");
    }

    public void RemoveMapMarker3D(Here.Explore.Maui.Models.Maps.MapMarker3D marker)
    {
        if (_mapScene is null) throw new InvalidOperationException("MapService not initialized.");
        if (_markers3D.TryGetValue(marker, out var androidMarker3D))
        {
            _mapScene.RemoveMapMarker3d(androidMarker3D);
            _markers3D.Remove(marker);
        }
    }

    public Task<Here.Explore.Maui.Models.Maps.MapPickResult?> PickAsync(Point2D screenPoint)
    {
        // Will be implemented with full pick result handling
        return Task.FromResult<Here.Explore.Maui.Models.Maps.MapPickResult?>(null);
    }

    private static Here.Explore.Core.Color ToCoreColor(uint argb)
    {
        var a = (float)((argb >> 24) & 0xFF) / 255f;
        var r = (float)((argb >> 16) & 0xFF) / 255f;
        var g = (float)((argb >> 8) & 0xFF) / 255f;
        var b = (float)(argb & 0xFF) / 255f;
        return new Here.Explore.Core.Color(g, a, r, b);
    }

    private static Here.Explore.Maps.MapScheme ToAndroidMapScheme(Here.Explore.Maui.Models.Maps.MapScheme scheme) => scheme switch
    {
        Here.Explore.Maui.Models.Maps.MapScheme.NormalDay => Here.Explore.Maps.MapScheme.NormalDay!,
        Here.Explore.Maui.Models.Maps.MapScheme.NormalNight => Here.Explore.Maps.MapScheme.NormalNight!,
        Here.Explore.Maui.Models.Maps.MapScheme.HybridDay => Here.Explore.Maps.MapScheme.HybridDay!,
        Here.Explore.Maui.Models.Maps.MapScheme.SatelliteDay => Here.Explore.Maps.MapScheme.Satellite!, // Satellite (not SatelliteDay)
        Here.Explore.Maui.Models.Maps.MapScheme.TerrainDay => Here.Explore.Maps.MapScheme.NormalDay!, // No TerrainDay in binding
        _ => Here.Explore.Maps.MapScheme.NormalDay!,
    };
}

internal class SceneLoadCallback : Java.Lang.Object, Here.Explore.Maps.MapScene.ILoadSceneCallback
{
    private readonly TaskCompletionSource<bool> _tcs;
    public SceneLoadCallback(TaskCompletionSource<bool> tcs) => _tcs = tcs;

    public void OnLoadScene(Here.Explore.Maps.MapError? error)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"SceneLoadCallback.OnLoadScene called, error={(error is null ? "null" : error.Value.ToString())}");
        if (error is null)
            _tcs.SetResult(true);
        else
            _tcs.SetException(new Exception($"Scene load error: {error.Value}"));
    }
}
#endif