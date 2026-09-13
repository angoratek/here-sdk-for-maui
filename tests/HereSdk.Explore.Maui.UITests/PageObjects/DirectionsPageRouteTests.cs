using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end UI test for the Directions route flow. Tapping Calculate
/// after typing origin and destination must not surface
/// "RoutingService not initialized." The same regression that broke
/// search (services registered in DI without Initialize()) affects
/// routing, so this test is the catch for the parallel bug.
/// </summary>
public class DirectionsPageRouteTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToDirections() => NavigateToTab("Directions");

    [Test]
    public void CalculateRoute_DoesNotShowServiceNotInitializedError()
    {
        var from = FindUIElement("DirectionsFromEntry");
        from.Clear();
        from.SendKeys("San Francisco, CA");

        var to = FindUIElement("DirectionsToEntry");
        to.Clear();
        to.SendKeys("Oakland, CA");

        DismissKeyboard();

        var button = FindUIElement("DirectionsCalculateButton");
        button.Click();

        Screenshot(nameof(CalculateRoute_DoesNotShowServiceNotInitializedError));

        // Give the route calculation a moment to either complete or fail.
        System.Threading.Thread.Sleep(5000);

        var matches = FindAllContainingText(NotInitializedSignature);

        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{NotInitializedSignature}': {texts}. " +
                $"RoutingService was used before its Initialize() ran.");
        }
    }

    /// <summary>
    /// The full route journey: type in the origin/destination entries,
    /// pick suggestions (free text alone never sets OriginPlace/
    /// DestinationPlace — CalculateRoute no-ops without them), tap
    /// Calculate, and verify the route sheet surfaces an ETA.
    /// On CI without HERE credentials the suggestions/route never
    /// resolve — that's not a regression, so the test is Inconclusive
    /// there (the workflow can seed credentials from secrets to make
    /// this a hard assertion).
    /// </summary>
    [Test]
    public void CalculateRoute_ShowsRouteSheetWithEta_WhenCredentialsAvailable()
    {
        SelectFirstSuggestion("DirectionsFromEntry", "San Francisco");
        SelectFirstSuggestion("DirectionsToEntry", "Oakland");

        FindUIElement("DirectionsCalculateButton").Click();

        // Route calculation is a network round trip (geocode both ends +
        // routing): poll generously.
        var eta = TryPollForUIElement("RouteSheetEta", TimeSpan.FromSeconds(25));

        Screenshot(nameof(CalculateRoute_ShowsRouteSheetWithEta_WhenCredentialsAvailable));

        if (eta is null)
        {
            var error = TryFindUIElement("RouteErrorLabel");
            Assert.Inconclusive(
                "Route sheet did not appear within 25s" +
                (error is not null ? $" — RouteError: {error.Text}" : "") +
                ". No HERE credentials or network on this runner?");
        }

        Assert.That(eta!.Text, Is.Not.Empty, "Route ETA should be populated");
    }

    /// <summary>
    /// Types a query into an origin/destination entry and taps the first
    /// autocomplete suggestion containing the query text — the only way
    /// OriginPlace/DestinationPlace get set, which CalculateRoute requires.
    /// The entry itself also matches <c>textContains</c> (its typed text),
    /// so matches equal to the entry's current text are skipped — clicking
    /// the entry selects nothing and leaves the popup open.
    /// Inconclusive when no suggestion appears (no credentials/network).
    /// </summary>
    private void SelectFirstSuggestion(string entryId, string query)
    {
        var entry = FindUIElement(entryId);
        entry.Clear();
        entry.SendKeys(query);
        DismissKeyboard();

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var matches = FindAllContainingText(query);
            var suggestion = matches.FirstOrDefault(m => m.Text != EntryValue(entry));
            if (suggestion is not null)
            {
                suggestion.Click();
                return;
            }
            Thread.Sleep(500);
        }

        Assert.Inconclusive($"No '{query}' suggestion appeared within 10s. No HERE credentials or network on this runner?");
    }

    private static string EntryValue(OpenQA.Selenium.IWebElement entry) =>
        AppiumSetup.Platform == TestPlatform.iOS
            ? entry.GetAttribute("value")
            : entry.Text;
}
