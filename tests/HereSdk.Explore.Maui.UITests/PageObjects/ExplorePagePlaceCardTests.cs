using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end test: tap the map to open the place card, tap "Get
/// Directions" and assert arrival on the Directions tab with the
/// destination pre-filled and the origin resolved from the device
/// location. This is the single most important user journey in the
/// RefApp — without it the search results are a dead end.
/// </summary>
public class ExplorePagePlaceCardTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToExplore() => NavigateToTab("Explore");

    [Test]
    public void MapTap_PlaceCard_TapGetDirections_NavigatesToDirectionsWithDestination()
    {
        var map = FindUIElement("ExploreMapView");

        // Element tap = tap at the map's center (coordinate TouchActions
        // crash WDA on iOS 26). The place card appears via the
        // tap-to-geocode path — a text search only drops result markers
        // and never opens the card. TapMapForPlaceCard polls and retries
        // the tap once (a tap can be consumed dismissing a stale card).
        var directionsCta = TapMapForPlaceCard(map);
        if (directionsCta is null)
        {
            Screenshot(nameof(MapTap_PlaceCard_TapGetDirections_NavigatesToDirectionsWithDestination) + "_noCard");
            Assert.Fail("Place card did not appear after tapping the map — reverse geocoding " +
                "or the place card sheet expansion failed");
        }

        directionsCta!.Click();

        // Shell tab switch + current-location lookup + route calculation.
        // Text is read via GetTextStaleSafe: the sheet's native views can be
        // recreated while the tab switch + route layout settle, which makes
        // a plain .Text read throw StaleElementReferenceException (seen on
        // CI's slower emulator).
        Assert.That(GetTextStaleSafe("DirectionsToEntry", 15), Is.Not.Empty,
            "DirectionsToEntry was not pre-filled with the destination");

        // The origin must be resolved from the device location automatically.
        Assert.That(GetTextStaleSafe("DirectionsFromEntry", 15), Is.Not.Empty,
            "DirectionsFromEntry was not pre-filled with the current location");
    }

    private void AssertNoElementContains(string substring)
    {
        var matches = FindAllContainingText(substring);
        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{substring}': {texts}. " +
                $"This indicates a service was used before its Initialize() ran.");
        }
    }
}