using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Cross-platform HERE map view control.
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

    /// <summary>Gets the map service for camera control, markers, and scene management.</summary>
    public IMapService Map => _mapService.Value;

    private Lazy<IMapService> _mapService;

    /// <summary>Initializes a new instance of the <see cref="HereMapView"/> class.</summary>
    public HereMapView()
    {
        _mapService = new Lazy<IMapService>(() =>
        {
            // Prefer the handler's MapService (which has the platform view initialized)
            if (Handler is Handlers.HereMapViewHandler handler && handler.MapService is not null)
                return handler.MapService;

            // Fallback to DI-resolved service
            if (Handler?.MauiContext?.Services is not null)
                return Handler.MauiContext.Services.GetRequiredService<IMapService>();

            throw new InvalidOperationException("MapService not available. Ensure HERE SDK is initialized.");
        });
    }

    /// <summary>Raised when the camera position changes.</summary>
    public event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;

    /// <summary>Raised when the map is idle (rendering and loading complete).</summary>
    public event EventHandler? MapIdle;

    /// <summary>Raised when the user taps on the map.</summary>
    public event EventHandler<MapTappedEventArgs>? MapTapped;

    /// <summary>Raised when the user double-taps on the map.</summary>
    public event EventHandler<MapDoubleTappedEventArgs>? MapDoubleTapped;

    /// <summary>Raised when the user long-presses on the map.</summary>
    public event EventHandler<MapLongPressedEventArgs>? MapLongPressed;

    /// <summary>Raised when the user pans (drags) the map.</summary>
    public event EventHandler<MapPannedEventArgs>? MapPanned;

    /// <summary>Raised when the user pinch-zooms or rotates the map.</summary>
    public event EventHandler<MapPinchRotatedEventArgs>? MapPinchRotated;

    internal void RaiseCameraStateChanged(CameraStateChangedEventArgs e) => CameraStateChanged?.Invoke(this, e);
    internal void RaiseMapIdle() => MapIdle?.Invoke(this, EventArgs.Empty);
    internal void RaiseMapTapped(MapTappedEventArgs e) => MapTapped?.Invoke(this, e);
    internal void RaiseMapDoubleTapped(MapDoubleTappedEventArgs e) => MapDoubleTapped?.Invoke(this, e);
    internal void RaiseMapLongPressed(MapLongPressedEventArgs e) => MapLongPressed?.Invoke(this, e);
    internal void RaiseMapPanned(MapPannedEventArgs e) => MapPanned?.Invoke(this, e);
    internal void RaiseMapPinchRotated(MapPinchRotatedEventArgs e) => MapPinchRotated?.Invoke(this, e);
}