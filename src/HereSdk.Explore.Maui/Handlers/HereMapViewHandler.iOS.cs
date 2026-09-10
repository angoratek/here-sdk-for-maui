#pragma warning disable CS1591
#if IOS
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;
using Here.Explore.iOS;
using Foundation;

namespace Here.Explore.Maui.Handlers;

public partial class HereMapViewHandler
{
    private UIKit.UIView? _platformView;
    private HereMapBridgeView? _bridgeView;
    private HereGestures? _gestures;

    // Gesture delegate handlers MUST be kept alive in fields: the Swift side
    // (HereGestures) holds them only weakly, so an unreferenced handler is
    // GC-collected and the map's tap/long-press/double-tap callbacks go
    // silently dead.
    private TapDelegateHandler? _tapDelegate;
    private LongPressDelegateHandler? _longPressDelegate;
    private DoubleTapDelegateHandler? _doubleTapDelegate;

    protected override UIKit.UIView CreatePlatformView()
    {
        Console.WriteLine("[REFAPP_DIAG] iOS CreatePlatformView started");
        _bridgeView = HereMapBridgeView.Create();

        var mapService = new MapService();
        var camera = new HereMapCamera(_bridgeView);
        var scene = new HereMapScene(_bridgeView);
        _gestures = new HereGestures(_bridgeView);
        mapService.Initialize(camera, scene, _gestures, _bridgeView);
        _mapService = mapService;
        Console.WriteLine("[REFAPP_DIAG] iOS MapService initialized");

        // Wire up camera state changes and map idle (raised on IMapService events)
        camera.SetCameraUpdatedHandler((lat, lon, zoom, bearing, tilt) =>
            mapService.RaiseCameraStateChanged(
                new CameraStateChangedEventArgs(new GeoCoordinates(lat, lon), zoom, bearing, tilt)));
        _bridgeView.SetMapIdleHandler(() => mapService.RaiseMapIdle());
        Console.WriteLine("[REFAPP_DIAG] iOS camera/idle handlers wired");

        // Wire up gesture events
        _tapDelegate = new TapDelegateHandler(this);
        _longPressDelegate = new LongPressDelegateHandler(this);
        _doubleTapDelegate = new DoubleTapDelegateHandler(this);
        _gestures.SetTapDelegate(_tapDelegate);
        _gestures.SetLongPressDelegate(_longPressDelegate);
        _gestures.SetDoubleTapDelegate(_doubleTapDelegate);
        Console.WriteLine("[REFAPP_DIAG] iOS gesture delegates wired");

        // Load the initial map scene. The MapScheme property defaults to NormalDay,
        // so XAML setting MapScheme="NormalDay" never triggers the property changed callback.
        var initialScheme = VirtualView?.MapScheme ?? Models.Maps.MapScheme.NormalDay;
        Console.WriteLine($"[REFAPP_DIAG] iOS loading initial scene: {initialScheme}");
        _ = LoadInitialSceneAsync(initialScheme);

        // The bridge view exposes the MapView as a UIView via PlatformView
        _platformView = _bridgeView.PlatformView ?? new UIKit.UIView(CoreGraphics.CGRect.Empty);
        Console.WriteLine("[REFAPP_DIAG] iOS CreatePlatformView completed");
        return _platformView;
    }

    protected override void DisconnectHandler(UIKit.UIView platformView)
    {
        if (_gestures is not null)
        {
            _gestures.SetTapDelegate(null!);
            _gestures.SetLongPressDelegate(null!);
            _gestures.SetDoubleTapDelegate(null!);
        }
        (_mapService as MapService)?.Dispose();
        _mapService = null;
        _gestures = null;
        _tapDelegate = null;
        _longPressDelegate = null;
        _doubleTapDelegate = null;
        _bridgeView = null;
        _platformView = null;
        base.DisconnectHandler(platformView);
    }

    internal void OnMapPinched(double scale)
    {
        if (_mapService is MapService ms)
        {
            var coordinates = new GeoCoordinates(0, 0); // TODO: get actual center from camera
            var args = new MapPinchedEventArgs(scale, coordinates);
            ms.RaiseMapPinched(args);
        }
    }

    internal void OnMapTapped(double originX, double originY)
    {
        Console.WriteLine($"[REFAPP_DIAG] iOS OnMapTapped: originX={originX}, originY={originY}");
        if (_mapService is MapService ms)
        {
            var coordinates = _bridgeView?.ViewToGeoCoordinates(originX, originY);
            var geo = coordinates is not null
                ? new GeoCoordinates(coordinates.Latitude, coordinates.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Point2D(originX, originY);
            Console.WriteLine($"[REFAPP_DIAG] iOS MapTapped raised: {geo.Latitude},{geo.Longitude}");
            ms.RaiseMapTapped(new MapTappedEventArgs(geo, screenPoint));
        }
    }

    internal void OnMapDoubleTapped(double originX, double originY)
    {
        Console.WriteLine($"[REFAPP_DIAG] iOS OnMapDoubleTapped: originX={originX}, originY={originY}");
        if (_mapService is MapService ms)
        {
            var coordinates = _bridgeView?.ViewToGeoCoordinates(originX, originY);
            var geo = coordinates is not null
                ? new GeoCoordinates(coordinates.Latitude, coordinates.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Point2D(originX, originY);
            ms.RaiseMapDoubleTapped(new MapTappedEventArgs(geo, screenPoint));
        }
    }

    internal void OnMapLongPressed(nint state, double originX, double originY)
    {
        Console.WriteLine($"[REFAPP_DIAG] iOS OnMapLongPressed: state={state}, originX={originX}, originY={originY}");
        if (_mapService is MapService ms)
        {
            var coordinates = _bridgeView?.ViewToGeoCoordinates(originX, originY);
            var geo = coordinates is not null
                ? new GeoCoordinates(coordinates.Latitude, coordinates.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Point2D(originX, originY);
            ms.RaiseMapLongPressed(new MapLongPressedEventArgs(geo, screenPoint));
        }
    }

    private async Task LoadInitialSceneAsync(Models.Maps.MapScheme scheme)
    {
        try
        {
            await _mapService!.LoadSceneAsync(scheme);
            Console.WriteLine($"[HereMapViewHandler] Initial scene loaded: {scheme}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HereMapViewHandler] Failed to load initial scene: {ex.Message}");
        }
    }
}

internal class TapDelegateHandler : HereTapDelegate
{
    private readonly HereMapViewHandler _handler;
    public TapDelegateHandler(HereMapViewHandler handler) { _handler = handler; }
    public override void OnTap(double originX, double originY) => _handler.OnMapTapped(originX, originY);
}

internal class LongPressDelegateHandler : HereLongPressDelegate
{
    private readonly HereMapViewHandler _handler;
    public LongPressDelegateHandler(HereMapViewHandler handler) { _handler = handler; }
    public override void OnLongPress(nint state, double originX, double originY) => _handler.OnMapLongPressed(state, originX, originY);
}

internal class DoubleTapDelegateHandler : HereDoubleTapDelegate
{
    private readonly HereMapViewHandler _handler;
    public DoubleTapDelegateHandler(HereMapViewHandler handler) { _handler = handler; }
    public override void OnDoubleTap(double originX, double originY) => _handler.OnMapDoubleTapped(originX, originY);
}
#endif