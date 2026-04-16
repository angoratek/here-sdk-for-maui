using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Cross-platform HERE map view control.
/// Map events are exposed via <see cref="Map"/> (IMapService).
/// </summary>
public class HereMapView : View, IHereMapView
{
    /// <summary>Bindable property for <see cref="CameraTarget"/>.</summary>
    public static readonly BindableProperty CameraTargetProperty =
        BindableProperty.Create(nameof(CameraTarget), typeof(GeoCoordinates), typeof(HereMapView));

    /// <summary>Bindable property for <see cref="MapScheme"/>.</summary>
    public static readonly BindableProperty MapSchemeProperty =
        BindableProperty.Create(nameof(MapScheme), typeof(MapScheme), typeof(HereMapView),
            MapScheme.NormalDay);

    /// <summary>Gets or sets the geographic coordinates the camera is centered on.</summary>
    public GeoCoordinates CameraTarget
    {
        get => (GeoCoordinates)GetValue(CameraTargetProperty);
        set => SetValue(CameraTargetProperty, value);
    }

    /// <summary>Gets or sets the map visual scheme (day, night, satellite, etc.).</summary>
    public MapScheme MapScheme
    {
        get => (MapScheme)GetValue(MapSchemeProperty);
        set => SetValue(MapSchemeProperty, value);
    }

    /// <summary>Gets the map service for camera control, markers, scene management, and events.</summary>
    public IMapService Map => _mapService.Value;

    private Lazy<IMapService> _mapService;

    /// <summary>Initializes a new instance of the <see cref="HereMapView"/> class.</summary>
    public HereMapView()
    {
        _mapService = new Lazy<IMapService>(() =>
        {
            // The handler creates the MapService and exposes it via the handler's MapService property.
            // We use the MAUI handler infrastructure to resolve it without coupling to a specific handler type.
            if (Handler is Handlers.HereMapViewHandler h && h.MapService is not null)
                return h.MapService;

            throw new InvalidOperationException(
                "MapService not available. Ensure the HereMapView has been added to the visual tree and HERE SDK is initialized.");
        });
    }
}