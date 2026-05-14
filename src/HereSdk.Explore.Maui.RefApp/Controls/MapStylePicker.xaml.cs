using System.Windows.Input;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.RefApp.Controls;

public partial class MapStylePicker : Border
{
    public static readonly BindableProperty SelectedSchemeProperty =
        BindableProperty.Create(nameof(SelectedScheme), typeof(MapScheme), typeof(MapStylePicker), MapScheme.NormalDay, BindingMode.TwoWay);

    public static readonly BindableProperty ChangeMapSchemeCommandProperty =
        BindableProperty.Create(nameof(ChangeMapSchemeCommand), typeof(ICommand), typeof(MapStylePicker));

    public static readonly BindableProperty IsDropdownExpandedProperty =
        BindableProperty.Create(nameof(IsDropdownExpanded), typeof(bool), typeof(MapStylePicker), false);

    public event EventHandler<MapScheme>? SchemeChanged;

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

    public bool IsDropdownExpanded
    {
        get => (bool)GetValue(IsDropdownExpandedProperty);
        set => SetValue(IsDropdownExpandedProperty, value);
    }

    public List<MapSchemeItem> MapSchemes { get; } = new()
    {
        new(MapScheme.NormalDay, "Normal Day", "☀️"),
        new(MapScheme.NormalNight, "Normal Night", "🌙"),
        new(MapScheme.HybridDay, "Hybrid Day", "🛰️"),
        new(MapScheme.SatelliteDay, "Satellite", "🌍"),
        new(MapScheme.TerrainDay, "Terrain", "🏔"),
    };

    public ICommand ToggleDropdownCommand { get; }
    public ICommand SelectSchemeCommand { get; }

    public MapStylePicker()
    {
        ToggleDropdownCommand = new Command(ToggleDropdown);
        SelectSchemeCommand = new Command<MapSchemeItem>(SelectScheme);
        InitializeComponent();
        BindingContext = this;
    }

    private void ToggleDropdown()
    {
        IsDropdownExpanded = !IsDropdownExpanded;
    }

    private void SelectScheme(MapSchemeItem? item)
    {
        if (item is null) return;
        SelectedScheme = item.Scheme;
        ChangeMapSchemeCommand?.Execute(item.Scheme);
        SchemeChanged?.Invoke(this, item.Scheme);
        IsDropdownExpanded = false;
    }
}

public record MapSchemeItem(MapScheme Scheme, string Label, string Icon);
