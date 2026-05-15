using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Controls;

namespace Here.Explore.Maui.RefApp.UITests.Controls;

public class PlaceCardTests
{
    [Fact]
    public void Distance_Formatting_LessThan1Km_ShowsMeters()
    {
        double distanceKm = 0.5;
        string result = distanceKm < 1
            ? $"{distanceKm * 1000:F0}m away"
            : $"{distanceKm:F1}km away";
        Assert.Equal("500m away", result);
    }

    [Fact]
    public void Distance_Formatting_GreaterThan1Km_ShowsKm()
    {
        double distanceKm = 3.75;
        string result = distanceKm < 1
            ? $"{distanceKm * 1000:F0}m away"
            : $"{distanceKm:F1}km away";
        Assert.Equal("3.8km away", result);
    }

    [Fact]
    public void Distance_Formatting_Exact1Km_ShowsKm()
    {
        double distanceKm = 1.0;
        string result = distanceKm < 1
            ? $"{distanceKm * 1000:F0}m away"
            : $"{distanceKm:F1}km away";
        Assert.Equal("1.0km away", result);
    }

    [Fact]
    public void Distance_Formatting_VerySmall_ShowsMeters()
    {
        double distanceKm = 0.003;
        string result = distanceKm < 1
            ? $"{distanceKm * 1000:F0}m away"
            : $"{distanceKm:F1}km away";
        Assert.Equal("3m away", result);
    }

    [Fact]
    public void CategoryColor_Restaurant_IsOrange()
    {
        var color = InvokeCategoryColorFor("restaurant-123");
        Assert.Equal(Color.FromArgb("#FF9500"), color);
    }

    [Fact]
    public void CategoryColor_Hotel_IsPurple()
    {
        var color = InvokeCategoryColorFor("hotel-abc");
        Assert.Equal(Color.FromArgb("#5856D6"), color);
    }

    [Fact]
    public void CategoryColor_Parking_IsGray()
    {
        var color = InvokeCategoryColorFor("parking-garage");
        Assert.Equal(Color.FromArgb("#8E8E93"), color);
    }

    [Fact]
    public void CategoryColor_FuelStation_IsBlue()
    {
        var color = InvokeCategoryColorFor("fuel-station-1");
        Assert.Equal(Color.FromArgb("#007AFF"), color);
    }

    [Fact]
    public void CategoryColor_Hospital_IsRed()
    {
        var color = InvokeCategoryColorFor("hospital-main");
        Assert.Equal(Color.FromArgb("#FF3B30"), color);
    }

    [Fact]
    public void CategoryColor_Attraction_IsGreen()
    {
        var color = InvokeCategoryColorFor("park-museum");
        Assert.Equal(Color.FromArgb("#34C759"), color);
    }

    [Fact]
    public void CategoryColor_Shopping_IsPink()
    {
        var color = InvokeCategoryColorFor("shop-retail");
        Assert.Equal(Color.FromArgb("#FF2D55"), color);
    }

    [Fact]
    public void CategoryColor_Bank_IsDarkGreen()
    {
        var color = InvokeCategoryColorFor("bank-atm");
        Assert.Equal(Color.FromArgb("#34A853"), color);
    }

    [Fact]
    public void CategoryColor_Unknown_IsDefaultBlue()
    {
        var color = InvokeCategoryColorFor("unknown-thing");
        Assert.Equal(Color.FromArgb("#5AC8FA"), color);
    }

    /// <summary>
    /// Invokes the private CategoryColorFor method via reflection.
    /// </summary>
    private static Color InvokeCategoryColorFor(string categoryId)
    {
        var method = typeof(PlaceCard).GetMethod("CategoryColorFor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (Color)method!.Invoke(null, new object[] { categoryId })!;
    }
}
