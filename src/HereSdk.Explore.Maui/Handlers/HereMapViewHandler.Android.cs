#if ANDROID
using Here.Explore.Maui.Controls;
using Com.Here.Sdk.Mapview;

namespace Here.Explore.Maui.Handlers;

public partial class HereMapViewHandler
{
    private MapView? _platformView;

    protected override object CreatePlatformView()
    {
        _platformView = new MapView(Platform.AppContext);
        return _platformView;
    }

    private static void MapCameraTarget(IHereMapView view, HereMapViewHandler handler)
    {
        if (handler._platformView is null) return;
        var target = view.CameraTarget;
        handler._platformView.Camera.Target = new Com.Here.Sdk.Core.GeoCoordinates(target.Latitude, target.Longitude);
    }

    private static void MapMapScheme(IHereMapView view, HereMapViewHandler handler)
    {
        if (handler._platformView is null) return;
        // Map scheme loading will be implemented in Phase 1
    }
}
#endif