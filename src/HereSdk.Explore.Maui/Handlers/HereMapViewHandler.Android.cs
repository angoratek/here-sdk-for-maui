#pragma warning disable CS1591
#if ANDROID
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Handlers;

public partial class HereMapViewHandler
{
    private Here.Explore.Maps.MapView? _platformView;
    private Here.Explore.Maps.MapCamera? _camera;
    private Here.Explore.Maps.MapScene? _scene;
    private Here.Explore.Gestures.Gestures? _gestures;

    protected override Android.Views.View CreatePlatformView()
    {
        _platformView = new Here.Explore.Maps.MapView(Platform.AppContext);
        _platformView.OnCreate(null);

        _camera = _platformView.Camera;
        _scene = _platformView.MapScene;
        _gestures = _platformView.Gestures;

        var mapService = new MapService();
        mapService.Initialize(_platformView);
        _mapService = mapService;

        // Wire up camera state changes
        _camera?.AddListener(new CameraListener(this));
        Android.Util.Log.Debug("REFAPP_DIAG", $"Camera listener wired, camera={(_camera is null ? "null" : "ok")}");

        // Wire up map idle
        var hereMap = _platformView.HereMap;
        Android.Util.Log.Debug("REFAPP_DIAG", $"HereMap={(hereMap is null ? "null" : "ok")}");
        if (hereMap is not null)
        {
            hereMap.AddMapIdleListener(new IdleListener(this));
            Android.Util.Log.Debug("REFAPP_DIAG", "Map idle listener wired");
        }

        // Wire up gesture events
        _gestures!.TapListener = new TapListener(this);
        _gestures.LongPressListener = new LongPressListener(this);
        _gestures.PinchRotateListener = new PinchRotateListener(this);

        // Start rendering
        _platformView.OnResume();
        Android.Util.Log.Debug("REFAPP_DIAG", $"MapView.OnResume called, camera={(_camera is null ? "null" : "ok")}");

        // Set initial camera target with zoom level
        if (VirtualView?.CameraTarget is { } target && _camera is not null)
        {
            Android.Util.Log.Debug("REFAPP_DIAG", $"Setting camera target: {target.Latitude},{target.Longitude} with zoom 10");
            var geoCoords = new Here.Explore.Core.GeoCoordinates(target.Latitude, target.Longitude);
            _camera.LookAt(geoCoords, new Here.Explore.Core.GeoOrientationUpdate(new Here.Explore.Core.GeoOrientation(0, 0)),
                new Here.Explore.Maps.MapMeasure(Here.Explore.Maps.MapMeasure.Kind.ZoomLevel!, 10.0));
        }
        else
        {
            Android.Util.Log.Debug("REFAPP_DIAG", $"No camera target set, VirtualView={VirtualView}");
        }

        // Load the initial map scene (XAML defaults don't trigger property changed callbacks)
        var initialScheme = VirtualView?.MapScheme ?? Models.Maps.MapScheme.NormalDay;
        Android.Util.Log.Debug("REFAPP_DIAG", $"Loading initial scene: {initialScheme}");
        _ = LoadInitialSceneAsync(initialScheme);

        return _platformView;
    }

    protected override void DisconnectHandler(Android.Views.View platformView)
    {
        if (_platformView is not null)
        {
            _platformView.OnPause();
            _platformView.OnDestroy();
        }
        if (_gestures is not null)
            _gestures.TapListener = null;
        (_mapService as MapService)?.Dispose();
        _mapService = null;
        _platformView?.Dispose();
        _camera = null;
        _scene = null;
        _gestures = null;
        base.DisconnectHandler(platformView);
    }

    internal void OnCameraStateChanged(Here.Explore.Maps.MapCamera.State state)
    {
        if (_mapService is MapService ms)
        {
            var args = new CameraStateChangedEventArgs(
                new Models.GeoCoordinates(state.TargetCoordinates.Latitude, state.TargetCoordinates.Longitude),
                state.ZoomLevel,
                state.OrientationAtTarget.Bearing,
                state.OrientationAtTarget.Tilt);
            ms.RaiseCameraStateChanged(args);
        }
    }

    internal void OnMapTapped(Here.Explore.Core.Point2D point)
    {
        if (_mapService is MapService ms)
        {
            var geoCoords = _platformView?.ViewToGeoCoordinates(point);
            var coordinates = geoCoords is not null
                ? new GeoCoordinates(geoCoords.Latitude, geoCoords.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Models.Point2D(point.X, point.Y);
            var args = new MapTappedEventArgs(coordinates, screenPoint);
            ms.RaiseMapTapped(args);
        }
    }

    internal void OnMapLongPressed(Here.Explore.Gestures.GestureState state, Here.Explore.Core.Point2D point)
    {
        if (_mapService is MapService ms)
        {
            var geoCoords = _platformView?.ViewToGeoCoordinates(point);
            var coordinates = geoCoords is not null
                ? new GeoCoordinates(geoCoords.Latitude, geoCoords.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Models.Point2D(point.X, point.Y);
            var args = new MapLongPressedEventArgs(coordinates, screenPoint);
            ms.RaiseMapLongPressed(args);
        }
    }

    internal void OnMapPinchRotated(Here.Explore.Gestures.GestureState state, Here.Explore.Core.Point2D pivotPoint, Here.Explore.Core.Point2D focalPoint, double scale, Here.Explore.Core.Angle rotation)
    {
        if (_mapService is MapService ms)
        {
            var geoCoords = _platformView?.Camera?.GetState().TargetCoordinates;
            var center = geoCoords is not null
                ? new GeoCoordinates(geoCoords.Latitude, geoCoords.Longitude)
                : new GeoCoordinates(0, 0);
            var args = new MapPinchedEventArgs(scale, center);
            ms.RaiseMapPinched(args);
        }
    }

    internal void OnMapIdle()
    {
        if (_mapService is MapService ms)
            ms.RaiseMapIdle();
    }

    private async Task LoadInitialSceneAsync(Models.Maps.MapScheme scheme)
    {
        try
        {
            await _mapService!.LoadSceneAsync(scheme);
        }
        catch
        {
            // Scene load failures are non-fatal — the map will show a blank state
        }
    }
}

internal class CameraListener : Java.Lang.Object, Here.Explore.Maps.MapCameraDelegate
{
    private readonly HereMapViewHandler _handler;
    public CameraListener(HereMapViewHandler handler) => _handler = handler;

    public void OnMapCameraUpdated(Here.Explore.Maps.MapCamera.State state)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"CameraListener.OnMapCameraUpdated: lat={state.TargetCoordinates.Latitude}, lon={state.TargetCoordinates.Longitude}, zoom={state.ZoomLevel}");
        _handler.OnCameraStateChanged(state);
    }
}

internal class TapListener : Java.Lang.Object, Here.Explore.Gestures.MapTapDelegate
{
    private readonly HereMapViewHandler _handler;
    public TapListener(HereMapViewHandler handler) => _handler = handler;

    public void OnTap(Here.Explore.Core.Point2D point)
    {
        _handler.OnMapTapped(point);
    }
}

internal class LongPressListener : Java.Lang.Object, Here.Explore.Gestures.MapLongPressDelegate
{
    private readonly HereMapViewHandler _handler;
    public LongPressListener(HereMapViewHandler handler) => _handler = handler;

    public void OnLongPress(Here.Explore.Gestures.GestureState state, Here.Explore.Core.Point2D point)
    {
        _handler.OnMapLongPressed(state, point);
    }
}

internal class IdleListener : Java.Lang.Object, Here.Explore.Maps.MapIdleDelegate
{
    private readonly HereMapViewHandler _handler;
    public IdleListener(HereMapViewHandler handler) => _handler = handler;

    public void OnMapIdle()
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "IdleListener.OnMapIdle called");
        _handler.OnMapIdle();
    }

    public void OnMapBusy()
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "IdleListener.OnMapBusy called");
    }
}

internal class PinchRotateListener : Java.Lang.Object, Here.Explore.Gestures.MapPinchRotateDelegate
{
    private readonly HereMapViewHandler _handler;
    public PinchRotateListener(HereMapViewHandler handler) => _handler = handler;

    public void OnPinchRotate(Here.Explore.Gestures.GestureState state, Here.Explore.Core.Point2D pivotPoint, Here.Explore.Core.Point2D focalPoint, double scale, Here.Explore.Core.Angle rotation)
    {
        _handler.OnMapPinchRotated(state, pivotPoint, focalPoint, scale, rotation);
    }
}
#endif