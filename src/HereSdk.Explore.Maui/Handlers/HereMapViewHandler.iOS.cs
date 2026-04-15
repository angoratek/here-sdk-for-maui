#if IOS
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Services;
using Here.Explore.iOS;

namespace Here.Explore.Maui.Handlers;

public partial class HereMapViewHandler
{
    private UIKit.UIView? _platformView;
    private HereMapBridgeView? _bridgeView;
    private MapService? _mapService;

    protected override UIKit.UIView CreatePlatformView()
    {
        _bridgeView = HereMapBridgeView.Create();

        var mapService = new MapService();
        var camera = new HereMapCamera(_bridgeView);
        var scene = new HereMapScene(_bridgeView);
        var gestures = new HereGestures(_bridgeView);
        mapService.Initialize(camera, scene, gestures, _bridgeView);
        _mapService = mapService;

        // The bridge view exposes the MapView as a UIView via PlatformView
        _platformView = _bridgeView.PlatformView ?? new UIKit.UIView(CoreGraphics.CGRect.Empty);
        return _platformView;
    }

    protected override void DisconnectHandler(UIKit.UIView platformView)
    {
        _mapService?.Dispose();
        _mapService = null;
        _bridgeView = null;
        _platformView = null;
        base.DisconnectHandler(platformView);
    }

    public MapService? MapService => _mapService;
}
#endif