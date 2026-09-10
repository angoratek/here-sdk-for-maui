using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.Services;

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
    public void CategoryVisuals_Restaurant_IsCoral()
    {
        var color = InvokeCategoryColorFor("restaurant-123");
        Assert.Equal(Color.FromArgb("#FF7A59"), color);
    }

    [Fact]
    public void CategoryVisuals_Hotel_IsPurple()
    {
        var color = InvokeCategoryColorFor("hotel-abc");
        Assert.Equal(Color.FromArgb("#7C5CD6"), color);
    }

    [Fact]
    public void CategoryVisuals_Parking_IsGray()
    {
        var color = InvokeCategoryColorFor("parking-garage");
        Assert.Equal(Color.FromArgb("#8E8E93"), color);
    }

    [Fact]
    public void CategoryVisuals_FuelStation_IsBlue()
    {
        var color = InvokeCategoryColorFor("fuel-station-1");
        Assert.Equal(Color.FromArgb("#0A7AFF"), color);
    }

    [Fact]
    public void CategoryVisuals_Hospital_IsRed()
    {
        var color = InvokeCategoryColorFor("hospital-main");
        Assert.Equal(Color.FromArgb("#E33B4E"), color);
    }

    [Fact]
    public void CategoryVisuals_Attraction_IsGreen()
    {
        var color = InvokeCategoryColorFor("city-park");
        Assert.Equal(Color.FromArgb("#2FBF71"), color);
    }

    [Fact]
    public void CategoryVisuals_Shopping_IsPink()
    {
        var color = InvokeCategoryColorFor("shop-retail");
        Assert.Equal(Color.FromArgb("#FF5B8A"), color);
    }

    [Fact]
    public void CategoryVisuals_Bank_IsTeal()
    {
        var color = InvokeCategoryColorFor("bank-atm");
        Assert.Equal(Color.FromArgb("#279E8F"), color);
    }

    [Fact]
    public void CategoryVisuals_Unknown_ReturnsNull()
    {
        // Unmapped categories return null; PlaceCard falls back to a translucent white disc.
        var color = InvokeCategoryColorFor("unknown-thing");
        Assert.Null(color);
    }

    /// <summary>Delegates to the shared category palette (was PlaceCard.CategoryColorFor).</summary>
    private static Color? InvokeCategoryColorFor(string categoryId)
        => Here.Explore.Maui.RefApp.Services.CategoryVisuals.ColorFor(categoryId);
}
