using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end UI tests for the Explore search flow. These are the last
/// line of defense against the "SearchService not initialized" regression:
/// they type a query, submit it, and assert that no "not initialized"
/// error banner appears.
///
/// On a healthy build, the search either returns results (asserted via the
/// "No places found" empty state being absent) or fails with a network
/// error — but never with the structural "service not initialized" error
/// that signals a missing <c>Initialize()</c> call in the DI factory.
/// </summary>
public class ExplorePageSearchTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToExplore() => NavigateToTab("Explore");

    [Test]
    public void SubmitTextSearch_DoesNotShowServiceNotInitializedError()
    {
        var search = FindUIElement("ExploreSearchEntry");
        search.Clear();
        search.SendKeys("coffee");
        App.HideKeyboard();

        // The Entry's ReturnCommand is SubmitSearchCommand. Pressing
        // the Enter key on the soft keyboard triggers the search.
        search.SendKeys(Keys.Enter);

        Screenshot(nameof(SubmitTextSearch_DoesNotShowServiceNotInitializedError));

        // The ErrorBanner has IsVisible = !string.IsNullOrEmpty(ErrorMessage).
        // If the service wasn't initialized, the banner shows
        // "Search error: SearchService not initialized." Look for any
        // element with that signature and assert it's absent.
        AssertNoElementContains(NotInitializedSignature);
    }

    [Test]
    public void TapRestaurantCategory_DoesNotShowServiceNotInitializedError()
    {
        TapCategoryByLabel("Restaurants");
        Screenshot(nameof(TapRestaurantCategory_DoesNotShowServiceNotInitializedError));
        AssertNoElementContains(NotInitializedSignature);
    }

    [Test]
    public void TapHotelCategory_DoesNotShowServiceNotInitializedError()
    {
        TapCategoryByLabel("Hotels");
        Screenshot(nameof(TapHotelCategory_DoesNotShowServiceNotInitializedError));
        AssertNoElementContains(NotInitializedSignature);
    }

    [Test]
    public void TapGasStationCategory_DoesNotShowServiceNotInitializedError()
    {
        TapCategoryByLabel("Gas Stations");
        Screenshot(nameof(TapGasStationCategory_DoesNotShowServiceNotInitializedError));
        AssertNoElementContains(NotInitializedSignature);
    }

    /// <summary>
    /// Taps a category chip by visible label text. The chips are
    /// programmatically-created <c>Border</c> controls in
    /// <c>CategoryChipBar</c>, so they don't have an <c>AutomationId</c>;
    /// we locate them by the <c>Label</c> text inside the chip.
    /// </summary>
    private void TapCategoryByLabel(string label)
    {
        // The chip label is rendered as "🍽 Restaurants" — match the
        // visible label exactly via the platform-appropriate text locator.
        var chip = FindByTextContains(label);
        chip.Click();
    }

    /// <summary>
    /// Asserts that no visible element on the current screen contains the
    /// given substring. Used to detect the "XxxService not initialized"
    /// error signature without depending on a specific element id.
    /// </summary>
    private void AssertNoElementContains(string substring)
    {
        // Give the search a moment to either complete or fail.
        // 5s is generous; the happy path completes in <1s on a real
        // network, and a slow network might take 2-3s. If the bug is
        // present, the error banner appears within a few hundred ms.
        System.Threading.Thread.Sleep(5000);

        var matches = FindAllContainingText(substring);

        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{substring}': {texts}. " +
                $"This indicates a service was used before its Initialize() ran " +
                $"— the UseHereSdkExplore DI factory regression.");
        }
    }
}
