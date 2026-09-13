using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end test for tap-to-geocode: tapping the map drops a pin,
/// reverse-geocodes the coordinate via ISearchService.SearchAsync(GeoCoordinates),
/// and slides up the place card with the resolved address. Guards the
/// reverse-geocoding path added to both platform SearchServices.
/// </summary>
public class ExplorePageTapGeocodeTests : BaseTest
{
    [SetUp]
    public void NavigateToExplore() => NavigateToTab("Explore");

    [Test]
    public void MapTap_ReverseGeocodes_ShowsPlaceCard()
    {
        var map = FindUIElement("ExploreMapView");

        // Element tap = tap at the map's center. Coordinate TouchActions
        // crash WDA on iOS 26 ("unrecognized selector:
        // waitForQuiescenceIncludingAnimationsIdle:"), so avoid them.
        // TapMapForPlaceCard retries the tap once — a first tap can be
        // consumed dismissing a stale card left open by an earlier test.
        var directionsCta = TapMapForPlaceCard(map);
        if (directionsCta is null)
        {
            Screenshot(nameof(MapTap_ReverseGeocodes_ShowsPlaceCard) + "_noCard");
            Assert.Fail("Place card did not appear after tapping the map — reverse geocoding " +
                "or the place card sheet expansion failed");
        }

        // Dismiss: tapping the map again should close the card.
        map.Click();
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline && TryFindUIElement("PlaceCardDirectionsButton") is not null)
        {
            System.Threading.Thread.Sleep(500);
        }
        Assert.That(TryFindUIElement("PlaceCardDirectionsButton"), Is.Null,
            "Place card should be dismissed after a second map tap");
    }
}