using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Services;
using Microsoft.Maui.Handlers;

namespace Here.Explore.Maui.Handlers;

/// <summary>
/// Shared handler logic for HereMapView.
/// </summary>
public partial class HereMapViewHandler : ViewHandler<IHereMapView, object>
{
    public static IPropertyMapper<IHereMapView, HereMapViewHandler> PropertyMapper = new PropertyMapper<IHereMapView, HereMapViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IHereMapView.CameraTarget)] = MapCameraTarget,
        [nameof(IHereMapView.MapScheme)] = MapMapScheme,
    };

    public HereMapViewHandler() : base(PropertyMapper) { }

    private static void MapCameraTarget(IHereMapView view, HereMapViewHandler handler)
    {
        // Platform-specific implementation in partial classes
    }

    private static void MapMapScheme(IHereMapView view, HereMapViewHandler handler)
    {
        // Platform-specific implementation in partial classes
    }
}