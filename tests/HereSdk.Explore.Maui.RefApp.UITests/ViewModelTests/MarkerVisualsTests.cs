using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.RefApp.Services;
using Xunit;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

/// <summary>
/// Per-type marker visuals: search results, traffic incidents, and the fixed
/// pins (origin/destination) each get a distinct glyph + tint, so pins on the
/// map read as what they mark.
/// </summary>
public class MarkerVisualsTests
{
    #region ForPlace

    [Fact]
    public void ForPlace_RestaurantCategory_UsesRestaurantVisuals()
    {
        var place = new Place("id", "Pizzeria", new(52.5, 13.4),
            PrimaryCategory: new PlaceCategory("100-1000-0000", "Restaurant"));

        var (glyph, color) = MarkerVisuals.ForPlace(place);

        Assert.NotEqual(MarkerVisuals.GlyphPlace, glyph); // restaurant glyph, not the fallback
        Assert.Equal(CategoryVisuals.ColorFor("restaurant")!.ToUint(), color);
    }

    [Fact]
    public void ForPlace_CategoryNameFallback_UsesCategoryVisuals()
    {
        var place = new Place("id", "Cafe Central", new(52.5, 13.4),
            PrimaryCategory: new PlaceCategory("x", "cafe"));

        var (glyph, _) = MarkerVisuals.ForPlace(place);

        Assert.False(string.IsNullOrEmpty(glyph));
   }

    [Fact]
    public void ForPlace_NoCategory_FallsBackToAccentPlace()
    {
        var place = new Place("id", "Somewhere", new(52.5, 13.4));

        var (glyph, color) = MarkerVisuals.ForPlace(place);

        Assert.Equal(MarkerVisuals.GlyphPlace, glyph);
        Assert.Equal(MarkerVisuals.Accent, color);
    }

    #endregion

    #region ForIncident

    [Fact]
    public void ForIncident_ClosedImpact_UsesRedTint()
    {
        var incident = new TrafficIncident("i1", "Road closed", TrafficIncidentType.RoadClosure,
            TrafficIncidentImpact.Closed);

        var (glyph, color) = MarkerVisuals.ForIncident(incident);

        Assert.False(string.IsNullOrEmpty(glyph));
        Assert.Equal(0xFFE33B4E, color);
    }

    [Fact]
    public void ForIncident_MinorImpact_UsesGreenTint()
    {
        var incident = new TrafficIncident("i2", "Slow traffic", TrafficIncidentType.Congestion,
            TrafficIncidentImpact.Minor);

        var (_, color) = MarkerVisuals.ForIncident(incident);

        Assert.Equal(0xFF2FBF71, color);
    }

    [Fact]
    public void IncidentGlyph_DistinctTypes_ProduceDistinctGlyphs()
    {
        var glyphs = new[]
        {
            TrafficIncidentType.Accident,
            TrafficIncidentType.Congestion,
            TrafficIncidentType.Construction,
            TrafficIncidentType.RoadClosure,
            TrafficIncidentType.RoadHazard,
            TrafficIncidentType.MassTransit,
            TrafficIncidentType.Weather
        }.Select(MarkerVisuals.IncidentGlyph).ToHashSet();

        Assert.Equal(7, glyphs.Count);
    }

    #endregion

    #region Fixed pins

    [Fact]
    public void FixedPins_OriginAndDestination_AreVisuallyDistinct()
    {
        Assert.NotEqual(MarkerVisuals.Origin, MarkerVisuals.Accent);
        Assert.NotEqual(MarkerVisuals.Origin, MarkerVisuals.Selected);
        Assert.NotEqual(MarkerVisuals.Neutral, MarkerVisuals.Selected);
    }

    #endregion
}