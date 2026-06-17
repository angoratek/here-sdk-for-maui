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

        Assert.That(from.Text, Is.EqualTo("San Francisco, CA"));
        Assert.That(to.Text, Is.EqualTo("Oakland, CA"));
    }

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
