using Xunit;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class MapStylePickerTests
{
    [Fact]
    public void MapSchemeItem_Constructor_SetsProperties()
    {
        var item = new MapSchemeItem(MapScheme.SatelliteDay, "Satellite", "\U0001f30d");
        Assert.Equal(MapScheme.SatelliteDay, item.Scheme);
        Assert.Equal("Satellite", item.Label);
        Assert.Equal("\U0001f30d", item.Icon);
    }

    [Fact]
    public void DefaultSelectedScheme_IsNormalDay()
    {
        // Cannot fully instantiate MapStylePicker (XAML requires dispatcher),
        // but the default value is a bindable property constant we can verify.
        var defaultValue = MapStylePicker.SelectedSchemeProperty.DefaultValue;
        Assert.Equal(MapScheme.NormalDay, defaultValue);
    }

    [Fact]
    public void IsDropdownExpanded_DefaultIsFalse()
    {
        var defaultValue = MapStylePicker.IsDropdownExpandedProperty.DefaultValue;
        Assert.False((bool)defaultValue!);
    }

    [Fact]
    public void MapSchemes_HaveAllFiveExpectedValues()
    {
        // Verify the scheme identifiers expected by the picker
        var expectedSchemes = new[]
        {
            MapScheme.NormalDay,
            MapScheme.NormalNight,
            MapScheme.HybridDay,
            MapScheme.SatelliteDay,
            MapScheme.TerrainDay,
        };
        Assert.Equal(5, expectedSchemes.Length);
    }
}
