using Xunit;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class TransportModePickerTests
{
    [Fact]
    public void Modes_HasAllEight()
    {
        Assert.Equal(8, TransportModePicker.Modes.Length);
    }

    [Fact]
    public void Mode_0_IsCar()
    {
        Assert.Equal("Car", TransportModePicker.Modes[0].Label);
        Assert.Equal(0, TransportModePicker.Modes[0].ModeKey);
    }

    [Fact]
    public void Mode_1_IsTruck()
    {
        Assert.Equal("Truck", TransportModePicker.Modes[1].Label);
        Assert.Equal(1, TransportModePicker.Modes[1].ModeKey);
    }

    [Fact]
    public void Mode_2_IsPedestrian()
    {
        Assert.Equal("Pedestrian", TransportModePicker.Modes[2].Label);
        Assert.Equal(2, TransportModePicker.Modes[2].ModeKey);
    }

    [Fact]
    public void Mode_3_IsBicycle()
    {
        Assert.Equal("Bicycle", TransportModePicker.Modes[3].Label);
        Assert.Equal(3, TransportModePicker.Modes[3].ModeKey);
    }

    [Fact]
    public void Mode_4_IsScooter()
    {
        Assert.Equal("Scooter", TransportModePicker.Modes[4].Label);
        Assert.Equal(4, TransportModePicker.Modes[4].ModeKey);
    }

    [Fact]
    public void Mode_5_IsBus()
    {
        Assert.Equal("Bus", TransportModePicker.Modes[5].Label);
        Assert.Equal(5, TransportModePicker.Modes[5].ModeKey);
    }

    [Fact]
    public void Mode_6_IsTaxi()
    {
        Assert.Equal("Taxi", TransportModePicker.Modes[6].Label);
        Assert.Equal(6, TransportModePicker.Modes[6].ModeKey);
    }

    [Fact]
    public void Mode_7_IsTransit()
    {
        Assert.Equal("Transit", TransportModePicker.Modes[7].Label);
        Assert.Equal(7, TransportModePicker.Modes[7].ModeKey);
    }

    [Fact]
    public void AllModes_HaveUniqueKeys()
    {
        var keys = TransportModePicker.Modes.Select(m => m.ModeKey).ToList();
        Assert.Equal(8, keys.Distinct().Count());
    }

    [Fact]
    public void AllModes_HaveIcons()
    {
        foreach (var mode in TransportModePicker.Modes)
        {
            Assert.False(string.IsNullOrEmpty(mode.Icon), $"Mode {mode.Label} missing icon");
        }
    }

    [Fact]
    public void AllModes_HaveLabels()
    {
        foreach (var mode in TransportModePicker.Modes)
        {
            Assert.False(string.IsNullOrEmpty(mode.Label), $"Mode {mode.ModeKey} missing label");
        }
    }
}
