using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies that tapping the "More Settings →" button on the Tools
/// page navigates to the Settings page, and that the Back button on
/// the Settings page returns to Tools. This is the entry path that
/// the entire Settings page sits behind, and was previously untested.
/// </summary>
public class SettingsPageNavigationTests : BaseTest
{
    [SetUp]
    public void NavigateToTools() => NavigateToTab("Tools");

    [Test]
    public void TapMoreSettings_NavigatesToSettingsPage()
    {
        var moreSettings = FindUIElement("ToolsMoreSettingsButton");
        moreSettings.Click();

        Screenshot(nameof(TapMoreSettings_NavigatesToSettingsPage));

        // The Settings page has a back button with id SettingsBackButton.
        // If the navigation worked, the back button is now in the tree.
        var back = TryFindUIElement("SettingsBackButton");
        Assert.That(back, Is.Not.Null,
            "SettingsBackButton not found — More Settings → did not navigate to SettingsPage");

        // And the about / legal / debug sections are present (text-based).
        var about = FindByTextContains("About");
        Assert.That(about.Displayed, Is.True);
    }

    [Test]
    public void TapBack_ReturnsToTools()
    {
        FindUIElement("ToolsMoreSettingsButton").Click();
        // Wait for navigation animation.
        System.Threading.Thread.Sleep(1500);

        var back = TryFindUIElement("SettingsBackButton");
        Assert.That(back, Is.Not.Null, "SettingsBackButton not found");
        back!.Click();

        Screenshot(nameof(TapBack_ReturnsToTools));

        // On Tools, ToolsMoreSettingsButton should be visible again.
        var moreSettings = TryFindUIElement("ToolsMoreSettingsButton");
        Assert.That(moreSettings, Is.Not.Null,
            "ToolsMoreSettingsButton not found after back — navigation did not return to Tools");
    }
}
