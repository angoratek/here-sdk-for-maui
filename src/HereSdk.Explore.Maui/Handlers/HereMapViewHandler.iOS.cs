#if IOS
using Here.Explore.Maui.Controls;

namespace Here.Explore.Maui.Handlers;

public partial class HereMapViewHandler
{
    private UIKit.UIView? _platformView;

    protected override object CreatePlatformView()
    {
        // MapView from NativeBridge — will be wired up in Phase 1
        _platformView = new UIKit.UIView(UIKit.UIRect.Zero);
        return _platformView;
    }

    private static void MapCameraTarget(IHereMapView view, HereMapViewHandler handler)
    {
        // iOS camera control will be implemented in Phase 1
    }

    private static void MapMapScheme(IHereMapView view, HereMapViewHandler handler)
    {
        // iOS map scheme loading will be implemented in Phase 1
    }
}
#endif