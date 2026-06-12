using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies the Tools tab's drawing tools and dark mode toggle. No assertion
/// on the map (same swiftshader caveat as the other pages).
/// </summary>
public class ToolsPageTests : BaseTest
{
    [Test]
    public void MarkerButton_IsPresent()
    {
        Screenshot(nameof(MarkerButton_IsPresent));

        Assert.That(FindUIElement("ToolsMarkerButton").Displayed, Is.True);
    }

    [Test]
    public void DarkModeToggle_TogglesState()
    {
        var toggle = FindUIElement("ToolsDarkModeToggle");
        var initial = toggle.Selected;
        toggle.Click();

        Screenshot(nameof(DarkModeToggle_TogglesState));

        Assert.That(toggle.Selected, Is.Not.EqualTo(initial));
    }
}
