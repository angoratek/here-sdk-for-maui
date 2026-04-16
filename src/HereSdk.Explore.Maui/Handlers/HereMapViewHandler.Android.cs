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
        _camera = _platformView.Camera;
        _scene = _platformView.MapScene;
        _gestures = _platformView.Gestures;

        var mapService = new MapService();
        mapService.Initialize(_platformView);
        _mapService = mapService;

        // Wire up camera state changes
        _camera?.AddListener(new CameraListener(this));

        // Wire up map idle
        _platformView.HereMap?.AddMapIdleListener(new IdleListener(this));

        // Wire up gesture events
        _gestures!.TapListener = new TapListener(this);

        return _platformView;
    }

    protected override void DisconnectHandler(Android.Views.View platformView)
    {
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
            var geoCoords = _platformView?.Camera?.GetState().TargetCoordinates;
            var coordinates = geoCoords is not null
                ? new GeoCoordinates(geoCoords.Latitude, geoCoords.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Models.Point2D(point.X, point.Y);
            var args = new MapTappedEventArgs(coordinates, screenPoint);
            ms.RaiseMapTapped(args);
        }
    }

    internal void OnMapIdle()
    {
        if (_mapService is MapService ms)
            ms.RaiseMapIdle();
    }
}

internal class CameraListener : Java.Lang.Object, Here.Explore.Maps.MapCameraDelegate
{
    private readonly HereMapViewHandler _handler;
    public CameraListener(HereMapViewHandler handler) => _handler = handler;

    public void OnMapCameraUpdated(Here.Explore.Maps.MapCamera.State state)
    {
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

internal class IdleListener : Java.Lang.Object, Here.Explore.Maps.MapIdleDelegate
{
    private readonly HereMapViewHandler _handler;
    public IdleListener(HereMapViewHandler handler) => _handler = handler;

    public void OnMapIdle()
    {
        _handler.OnMapIdle();
    }

    public void OnMapBusy() { }
}
#endif