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
    public void NavigateToTools()
    {
        NavigateToTab("Tools");
        ExpandToolsSettings();
    }

    [Test]
    public void TapMoreSettings_NavigatesToSettingsPage()
    {
        var moreSettings = FindUIElement("ToolsMoreSettingsButton");
        moreSettings.Click();

        Screenshot(nameof(TapMoreSettings_NavigatesToSettingsPage));

        // The Settings page has a back button with id SettingsBackButton.
        // If the navigation worked, the back button is now in the tree —
        // poll for it, the pushed-page navigation can take seconds on a
        // slow emulator.
        var back = WaitForUIElement("SettingsBackButton", 10);
        Assert.That(back, Is.Not.Null,
            "SettingsBackButton not found — More Settings → did not navigate to SettingsPage");

        // And the about / legal / debug sections are present (text-based).
        var about = FindByTextContains("About");
        Assert.That(about.Displayed, Is.True);

        // Navigate back so later fixtures find the Shell tab bar — the
        // pushed Settings page hides it, and with noReset=true the app
        // would otherwise stay here for the rest of the run.
        FindUIElement("SettingsBackButton").Click();
    }

    [Test]
    public void TapBack_ReturnsToTools()
    {
        FindUIElement("ToolsMoreSettingsButton").Click();

        // Poll for the pushed page — a fixed sleep races the navigation
        // animation on a slow emulator.
        var back = WaitForUIElement("SettingsBackButton", 10);
        back!.Click();

        Screenshot(nameof(TapBack_ReturnsToTools));

        // Back navigation creates a fresh ToolsPage with the Settings
        // section collapsed again — expand it so the button is in the tree.
        ExpandToolsSettings();
        var moreSettings = WaitForUIElement("ToolsMoreSettingsButton", 10);
        Assert.That(moreSettings, Is.Not.Null,
            "ToolsMoreSettingsButton not found after back — navigation did not return to Tools");
    }
}
