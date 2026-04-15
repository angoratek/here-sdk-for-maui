using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Services;
using Microsoft.Maui.Handlers;

namespace Here.Explore.Maui.Handlers;

/// <summary>
/// Shared handler logic for HereMapView.
/// Platform view type is declared in platform-specific partial classes.
/// </summary>
#if ANDROID
public partial class HereMapViewHandler : ViewHandler<IHereMapView, Android.Views.View>
#elif IOS
public partial class HereMapViewHandler : ViewHandler<IHereMapView, UIKit.UIView>
#else
public partial class HereMapViewHandler : ViewHandler<IHereMapView, object>
#endif
{
    /// <summary>Property mapper that maps IHereMapView bindable properties to handler actions.</summary>
    public static IPropertyMapper<IHereMapView, HereMapViewHandler> PropertyMapper = new PropertyMapper<IHereMapView, HereMapViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IHereMapView.CameraTarget)] = MapCameraTarget,
        [nameof(IHereMapView.MapScheme)] = MapMapScheme,
    };

    /// <summary>Initializes a new instance of the <see cref="HereMapViewHandler"/> class.</summary>
    public HereMapViewHandler() : base(PropertyMapper) { }

    /// <summary>Maps the CameraTarget property change to the platform map service.</summary>
    private static void MapCameraTarget(HereMapViewHandler handler, IHereMapView view)
    {
        // Handled by MapService directly
    }

    /// <summary>Maps the MapScheme property change to the platform map service.</summary>
    private static void MapMapScheme(HereMapViewHandler handler, IHereMapView view)
    {
        // Handled by MapService directly
    }
}