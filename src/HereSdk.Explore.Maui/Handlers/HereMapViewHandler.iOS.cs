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
        _bridgeView = HereMapBridgeView.Create();

        var mapService = new MapService();
        var camera = new HereMapCamera(_bridgeView);
        var scene = new HereMapScene(_bridgeView);
        _gestures = new HereGestures(_bridgeView);
        mapService.Initialize(camera, scene, _gestures, _bridgeView);
        _mapService = mapService;

        // Wire up gesture events
        _gestures.SetTapDelegate(new TapDelegateHandler(this));
        _gestures.SetLongPressDelegate(new LongPressDelegateHandler(this));
        _gestures.SetDoubleTapDelegate(new DoubleTapDelegateHandler(this));

        // Load the initial map scene. The MapScheme property defaults to NormalDay,
        // so XAML setting MapScheme="NormalDay" never triggers the property changed callback.
        var initialScheme = VirtualView?.MapScheme ?? Models.Maps.MapScheme.NormalDay;
        _ = LoadInitialSceneAsync(initialScheme);

        // The bridge view exposes the MapView as a UIView via PlatformView
        _platformView = _bridgeView.PlatformView ?? new UIKit.UIView(CoreGraphics.CGRect.Empty);
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

    internal void OnMapTapped(double originX, double originY)
    {
        if (_mapService is MapService ms)
        {
            var coordinates = new GeoCoordinates(0, 0); // TODO: convert screen point to geo coordinates
            var screenPoint = new Point2D(originX, originY);
            ms.RaiseMapTapped(new MapTappedEventArgs(coordinates, screenPoint));
        }
    }

    internal void OnMapDoubleTapped(double originX, double originY)
    {
        if (_mapService is MapService ms)
        {
            var coordinates = new GeoCoordinates(0, 0); // TODO: convert screen point to geo coordinates
            var screenPoint = new Point2D(originX, originY);
            ms.RaiseMapDoubleTapped(new MapTappedEventArgs(coordinates, screenPoint));
        }
    }

    internal void OnMapLongPressed(nint state, double originX, double originY)
    {
        if (_mapService is MapService ms)
        {
            var coordinates = new GeoCoordinates(0, 0); // TODO: convert screen point to geo coordinates
            var screenPoint = new Point2D(originX, originY);
            ms.RaiseMapLongPressed(new MapLongPressedEventArgs(coordinates, screenPoint));
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