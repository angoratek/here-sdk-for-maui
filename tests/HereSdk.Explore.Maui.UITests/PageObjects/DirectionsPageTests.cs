using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies the Directions tab's origin/destination entries and the calculate
/// button. Does not actually compute a route — the HERE SDK needs valid
/// access keys + a network connection for that, and CI runs without those.
/// </summary>
public class DirectionsPageTests : BaseTest
{
    [SetUp]
    public void NavigateToDirections() => NavigateToTab("Directions");

    [Test]
    public void DirectionsMapView_IsPresent()
    {
        Screenshot(nameof(DirectionsMapView_IsPresent));

        Assert.That(FindUIElement("DirectionsMapView").Displayed, Is.True);
    }

    [Test]
    public void OriginAndDestinationEntries_AcceptText()
    {
        var from = FindUIElement("DirectionsFromEntry");
        from.Clear();
        from.SendKeys("San Francisco, CA");

        var to = FindUIElement("DirectionsToEntry");
        to.Clear();
        to.SendKeys("Oakland, CA");

        Screenshot(nameof(OriginAndDestinationEntries_AcceptText));

        // On iOS, element.Text exposes the accessibility label (the
        // placeholder, e.g. "Origin"), not the typed value — read the
        // field's value attribute there instead.
        Assert.That(EntryText(from), Is.EqualTo("San Francisco, CA"));
        Assert.That(EntryText(to), Is.EqualTo("Oakland, CA"));
    }

    private static string EntryText(OpenQA.Selenium.IWebElement entry) =>
        AppiumSetup.Platform == TestPlatform.iOS
            ? entry.GetAttribute("value")
            : entry.Text;

    [Test]
    public void CalculateButton_IsDisplayedAfterOrigin()
    {
        var from = FindUIElement("DirectionsFromEntry");
        from.Clear();
        from.SendKeys("Times Square, New York");
        // Hide the keyboard so the floating button is visible.
        App.HideKeyboard();

        Screenshot(nameof(CalculateButton_IsDisplayedAfterOrigin));

        var button = FindUIElement("DirectionsCalculateButton");
        Assert.That(button.Displayed, Is.True);
    }
}
