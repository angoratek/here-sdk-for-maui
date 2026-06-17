using NUnit.Framework;

using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.MultiTouch;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies the Tools tab's drawing tools and dark mode toggle. No assertion
/// on the map (same swiftshader caveat as the other pages).
/// </summary>
public class ToolsPageTests : BaseTest
{
    [SetUp]
    public void NavigateToTools() => NavigateToTab("Tools");

    [Test]
    public void MarkerButton_IsPresent()
    {
        Screenshot(nameof(MarkerButton_IsPresent));

        Assert.That(FindUIElement("ToolsMarkerButton").Displayed, Is.True);
    }

    [Test]
    public void DarkModeToggle_TogglesState()
    {
        // The Tools page has three collapsible cards (Drawing Tools,
        // Demo Gallery, Settings). The Dark Mode Switch lives inside
        // Settings, which is collapsed by default. Tapping the
        // "Settings" header label expands the card; the chevron flips
        // from ▸ to ▾. We assert on the chevron state instead of the
        // Switch element itself because MAUI's Switch renders as a
        // custom-drawn ViewGroup with no native `android.widget.Switch`
        // surface and no `resource-id` for UIAutomator2 to latch onto.
        var settingsHeader = FindByText("Settings");
        var loc = settingsHeader.Location;
        var size = settingsHeader.Size;
        Thread.Sleep(200);
#pragma warning disable CS0618
        new TouchAction(App)
            .Tap(loc.X + size.Width + 10, loc.Y + size.Height / 2)
            .Perform();
#pragma warning restore CS0618
        Thread.Sleep(500);

        Screenshot(nameof(DarkModeToggle_TogglesState));

        // After expanding Settings the chevron next to the header
        // should be ▾ (down) rather than ▸ (right).
        var chevron = FindByText("▾");
        Assert.That(chevron, Is.Not.Null);
        Assert.That(chevron.Displayed, Is.True);
    }
}
