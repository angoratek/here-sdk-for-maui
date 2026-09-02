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
        // and never opens the card. 8s covers a slow first reverse geocode.
        map.Click();
        System.Threading.Thread.Sleep(8000);

        AssertNoElementContains(NotInitializedSignature);

        var directionsCta = TryFindUIElement("PlaceCardDirectionsButton");
        if (directionsCta is null)
        {
            Screenshot(nameof(MapTap_PlaceCard_TapGetDirections_NavigatesToDirectionsWithDestination) + "_noCard");
            Assert.Fail("Place card did not appear after tapping the map — reverse geocoding " +
                "or the place card sheet expansion failed");
        }

        directionsCta!.Click();

        // Shell tab switch + current-location lookup + route calculation.
        System.Threading.Thread.Sleep(5000);

        var toEntry = TryFindUIElement("DirectionsToEntry");
        Assert.That(toEntry, Is.Not.Null, "DirectionsToEntry not found after tapping Get Directions");
        Assert.That(toEntry!.Text, Is.Not.Empty,
            "DirectionsToEntry was not pre-filled with the destination");

        // The origin must be resolved from the device location automatically.
        var fromEntry = TryFindUIElement("DirectionsFromEntry");
        Assert.That(fromEntry, Is.Not.Null, "DirectionsFromEntry not found after tapping Get Directions");
        Assert.That(fromEntry!.Text, Is.Not.Empty,
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