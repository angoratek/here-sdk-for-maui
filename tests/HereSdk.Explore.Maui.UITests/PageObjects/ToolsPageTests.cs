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
        // The Tools panel has three collapsible cards (Drawing Tools,
        // Demo Gallery, Settings). The Dark Mode Switch lives inside
        // Settings, which is collapsed by default — and sits below the
        // Demo Gallery, off the bottom of the fully expanded sheet.
        // ExpandToolsSettings scrolls the header into view, taps it and
        // asserts the expansion deterministically via the scheme chips
        // (they only exist in the tree while the section is expanded).
        ExpandToolsSettings();

        Screenshot(nameof(DarkModeToggle_TogglesState));

        var schemeChip = WaitForUIElement("ToolsSchemeNormalDay", 10);
        Assert.That(schemeChip.Displayed, Is.True,
            "Settings section did not expand — ToolsSchemeNormalDay not found");
    }
}
