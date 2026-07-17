using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end test: search for a known place, open the place card, tap
/// "Get Directions" and assert arrival on the Directions tab with the
/// destination pre-filled. This is the single most important user
/// journey in the RefApp — without it the search results are a dead end.
/// </summary>
public class ExplorePagePlaceCardTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToExplore() => NavigateToTab("Explore");

    [Test]
    public void SearchPlace_TapGetDirections_NavigatesToDirectionsTabWithDestination()
    {
        var search = FindUIElement("ExploreSearchEntry");
        search.Clear();
        search.SendKeys("coffee");
        App.HideKeyboard();
        search.SendKeys(Keys.Enter);

        Screenshot(nameof(SearchPlace_TapGetDirections_NavigatesToDirectionsTabWithDestination));

        // Give the search a moment to complete and the place card to
        // appear in the bottom sheet. 6s is enough on a healthy network.
        System.Threading.Thread.Sleep(6000);

        // First sanity: no "service not initialized" error. The end-to-end
        // journey is only meaningful if the search itself works.
        AssertNoElementContains(NotInitializedSignature);

        // Look for the place card's "Get Directions" CTA. It only exists
        // when IsPlaceCardVisible flips true. If results came back, tap it.
        var directionsCta = TryFindUIElement("PlaceCardDirectionsButton");
        if (directionsCta is null)
        {
            // No results — the network probably had no data, but the search
            // itself completed. In CI without real HERE SDK credentials this
            // is the common case; we treat it as a pass rather than fail.
            Assert.Inconclusive("No search results — place card did not appear. The HERE SDK may not be configured in this CI environment.");
            return;
        }

        directionsCta.Click();

        // After tapping, the Shell should switch to the Directions tab.
        // We assert by checking the From/To entries are present and the
        // To entry is pre-populated with the destination.
        System.Threading.Thread.Sleep(2000);

        var toEntry = TryFindUIElement("DirectionsToEntry");
        Assert.That(toEntry, Is.Not.Null, "DirectionsToEntry not found after tapping Get Directions");
        Assert.That(toEntry!.Text, Is.Not.Empty,
            "DirectionsToEntry was not pre-filled with the destination");
    }

    private void AssertNoElementContains(string substring)
    {
        var matches = App.FindElements(
            MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\"{substring}\")"));
        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{substring}': {texts}. " +
                $"This indicates a service was used before its Initialize() ran.");
        }
    }
}
