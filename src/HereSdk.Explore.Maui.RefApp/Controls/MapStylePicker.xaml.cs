using System.Windows.Input;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class MapStylePicker : Border
{
    public static readonly BindableProperty SelectedSchemeProperty =
        BindableProperty.Create(nameof(SelectedScheme), typeof(MapScheme), typeof(MapStylePicker), MapScheme.NormalDay, BindingMode.TwoWay);

    public static readonly BindableProperty ChangeMapSchemeCommandProperty =
        BindableProperty.Create(nameof(ChangeMapSchemeCommand), typeof(ICommand), typeof(MapStylePicker));

    public MapScheme SelectedScheme
    {
        get => (MapScheme)GetValue(SelectedSchemeProperty);
        set => SetValue(SelectedSchemeProperty, value);
    }

    public ICommand? ChangeMapSchemeCommand
    {
        get => (ICommand?)GetValue(ChangeMapSchemeCommandProperty);
        set => SetValue(ChangeMapSchemeCommandProperty, value);
    }

    public List<MapScheme> MapSchemes { get; } = new()
    {
        MapScheme.NormalDay,
        MapScheme.NormalNight,
        MapScheme.HybridDay,
        MapScheme.SatelliteDay,
        MapScheme.TerrainDay
    };

    public MapStylePicker()
    {
        InitializeComponent();
        BindingContext = this;
    }
}
