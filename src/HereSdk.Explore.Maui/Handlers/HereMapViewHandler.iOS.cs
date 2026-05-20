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

    protected override UIKit.UIView CreatePlatformView()
    {
        System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] iOS CreatePlatformView started");
        _bridgeView = HereMapBridgeView.Create();

        var mapService = new MapService();
        var camera = new HereMapCamera(_bridgeView);
        var scene = new HereMapScene(_bridgeView);
        _gestures = new HereGestures(_bridgeView);
        mapService.Initialize(camera, scene, _gestures, _bridgeView);
        _mapService = mapService;
        System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] iOS MapService initialized");

        // Wire up gesture events
        _gestures.SetTapDelegate(new TapDelegateHandler(this));
        _gestures.SetLongPressDelegate(new LongPressDelegateHandler(this));
        _gestures.SetDoubleTapDelegate(new DoubleTapDelegateHandler(this));
        System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] iOS gesture delegates wired");

        // Load the initial map scene. The MapScheme property defaults to NormalDay,
        // so XAML setting MapScheme="NormalDay" never triggers the property changed callback.
        var initialScheme = VirtualView?.MapScheme ?? Models.Maps.MapScheme.NormalDay;
        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] iOS loading initial scene: {initialScheme}");
        _ = LoadInitialSceneAsync(initialScheme);

        // The bridge view exposes the MapView as a UIView via PlatformView
        _platformView = _bridgeView.PlatformView ?? new UIKit.UIView(CoreGraphics.CGRect.Empty);
        System.Diagnostics.Debug.WriteLine("[REFAPP_DIAG] iOS CreatePlatformView completed");
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
        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] iOS OnMapTapped: originX={originX}, originY={originY}");
        if (_mapService is MapService ms)
        {
            var coordinates = _bridgeView?.ViewToGeoCoordinates(originX, originY);
            var geo = coordinates is not null
                ? new GeoCoordinates(coordinates.Latitude, coordinates.Longitude)
                : new GeoCoordinates(0, 0);
            var screenPoint = new Point2D(originX, originY);
            System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] iOS MapTapped raised: {geo.Latitude},{geo.Longitude}");
            ms.RaiseMapTapped(new MapTappedEventArgs(geo, screenPoint));
        }
    }

    internal void OnMapDoubleTapped(double originX, double originY)
    {
        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] iOS OnMapDoubleTapped: originX={originX}, originY={originY}");
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
        System.Diagnostics.Debug.WriteLine($"[REFAPP_DIAG] iOS OnMapLongPressed: state={state}, originX={originX}, originY={originY}");
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
            System.Diagnostics.Debug.WriteLine($"[HereMapViewHandler] Initial scene loaded: {scheme}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HereMapViewHandler] Failed to load initial scene: {ex.Message}");
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