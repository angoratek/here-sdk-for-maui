using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Cross-platform HERE map view control.
/// </summary>
public class HereMapView : View, IHereMapView
{
    public static readonly BindableProperty CameraTargetProperty =
        BindableProperty.Create(nameof(CameraTarget), typeof(GeoCoordinates), typeof(HereMapView));

    public static readonly BindableProperty MapSchemeProperty =
        BindableProperty.Create(nameof(MapScheme), typeof(MapScheme), typeof(HereMapView),
            MapScheme.NormalDay);

    public GeoCoordinates CameraTarget
    {
        get => (GeoCoordinates)GetValue(CameraTargetProperty);
        set => SetValue(CameraTargetProperty, value);
    }

    public MapScheme MapScheme
    {
        get => (MapScheme)GetValue(MapSchemeProperty);
        set => SetValue(MapSchemeProperty, value);
    }

    public IMapService Map => _mapService.Value;

    private readonly Lazy<IMapService> _mapService = new(() =>
        Handler?.MauiContext?.Services.GetRequiredService<IMapService>()
        ?? throw new InvalidOperationException("MapService not available. Ensure HERE SDK is initialized."));

    public event EventHandler<CameraStateChangedEventArgs>? CameraStateChanged;
    public event EventHandler? MapIdle;

    internal void RaiseCameraStateChanged(CameraStateChangedEventArgs e) => CameraStateChanged?.Invoke(this, e);
    internal void RaiseMapIdle() => MapIdle?.Invoke(this, EventArgs.Empty);
}