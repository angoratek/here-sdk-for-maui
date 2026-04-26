using System.Windows.Input;

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

    public MapObjectsPanel()
    {
        InitializeComponent();
    }
}
