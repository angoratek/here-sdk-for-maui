using System.Windows.Input;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class MapObjectsPanel : Border
{
    public static readonly BindableProperty ToggleMarkersCommandProperty =
        BindableProperty.Create(nameof(ToggleMarkersCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty ToggleCirclesCommandProperty =
        BindableProperty.Create(nameof(ToggleCirclesCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty TogglePolylinesCommandProperty =
        BindableProperty.Create(nameof(TogglePolylinesCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty TogglePolygonsCommandProperty =
        BindableProperty.Create(nameof(TogglePolygonsCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty ClearAllCommandProperty =
        BindableProperty.Create(nameof(ClearAllCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty PanelVisibleProperty =
        BindableProperty.Create(nameof(PanelVisible), typeof(bool), typeof(MapObjectsPanel), false, BindingMode.TwoWay);

    public static readonly BindableProperty SetDrawingModeCommandProperty =
        BindableProperty.Create(nameof(SetDrawingModeCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public static readonly BindableProperty CancelDrawingCommandProperty =
        BindableProperty.Create(nameof(CancelDrawingCommand), typeof(ICommand), typeof(MapObjectsPanel));

    public ICommand? ToggleMarkersCommand
    {
        get => (ICommand?)GetValue(ToggleMarkersCommandProperty);
        set => SetValue(ToggleMarkersCommandProperty, value);
    }

    public ICommand? ToggleCirclesCommand
    {
        get => (ICommand?)GetValue(ToggleCirclesCommandProperty);
        set => SetValue(ToggleCirclesCommandProperty, value);
    }

    public ICommand? TogglePolylinesCommand
    {
        get => (ICommand?)GetValue(TogglePolylinesCommandProperty);
        set => SetValue(TogglePolylinesCommandProperty, value);
    }

    public ICommand? TogglePolygonsCommand
    {
        get => (ICommand?)GetValue(TogglePolygonsCommandProperty);
        set => SetValue(TogglePolygonsCommandProperty, value);
    }

    public ICommand? ClearAllCommand
    {
        get => (ICommand?)GetValue(ClearAllCommandProperty);
        set => SetValue(ClearAllCommandProperty, value);
    }

    public bool PanelVisible
    {
        get => (bool)GetValue(PanelVisibleProperty);
        set => SetValue(PanelVisibleProperty, value);
    }

    public ICommand? SetDrawingModeCommand
    {
        get => (ICommand?)GetValue(SetDrawingModeCommandProperty);
        set => SetValue(SetDrawingModeCommandProperty, value);
    }

    public ICommand? CancelDrawingCommand
    {
        get => (ICommand?)GetValue(CancelDrawingCommandProperty);
        set => SetValue(CancelDrawingCommandProperty, value);
    }

    public MapObjectsPanel()
    {
        InitializeComponent();
    }

    private void OnDrawMarkerClicked(object? sender, EventArgs e)
    {
#if ANDROID
        Android.Util.Log.Debug("REFAPP_DIAG", "OnDrawMarkerClicked");
#endif
        SetDrawingModeCommand?.Execute(DrawingMode.Marker);
    }

    private void OnDrawPolylineClicked(object? sender, EventArgs e)
    {
#if ANDROID
        Android.Util.Log.Debug("REFAPP_DIAG", "OnDrawPolylineClicked");
#endif
        SetDrawingModeCommand?.Execute(DrawingMode.Polyline);
    }

    private void OnDrawPolygonClicked(object? sender, EventArgs e)
    {
#if ANDROID
        Android.Util.Log.Debug("REFAPP_DIAG", "OnDrawPolygonClicked");
#endif
        SetDrawingModeCommand?.Execute(DrawingMode.Polygon);
    }

    private void OnCancelDrawingClicked(object? sender, EventArgs e)
    {
#if ANDROID
        Android.Util.Log.Debug("REFAPP_DIAG", "OnCancelDrawingClicked");
#endif
        CancelDrawingCommand?.Execute(null);
    }
}
