using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests;

/// <summary>
/// Base class for all page-object tests. Exposes the shared <see cref="App"/>
/// (AndroidDriver) and a small <see cref="FindUIElement"/> helper that locates
/// elements by their MAUI <c>AutomationId</c> (which UIAutomator2 surfaces as
/// a content-desc / accessibility-id).
/// </summary>
public abstract class BaseTest
{
    protected AppiumDriver App => AppiumSetup.App;

    /// <summary>
    /// Locates a UI element by its MAUI <c>AutomationId</c>. On Android the
    /// MAUI handler maps <c>AutomationId</c> to the native
    /// <c>resource-id</c> (e.g. <c>com.here.explore.maui.refapp:id/ExploreMapView</c>),
    /// NOT to <c>content-desc</c>, so we use <see cref="MobileBy.Id"/> rather
    /// than <see cref="MobileBy.AccessibilityId"/>. The accessibility-id
    /// locator would match against <c>content-desc</c>, which MAUI leaves
    /// for semantic labels like "Interactive map for exploring places".
    /// </summary>
    protected IWebElement FindUIElement(string automationId) =>
        App.FindElement(MobileBy.Id($"{AppiumSetup.AppPackage}:id/{automationId}"));

    /// <summary>
    /// Locates a UI element by its visible text. Used for elements that
    /// have no <c>AutomationId</c> (e.g. header <c>Label</c>s in
    /// collapsible cards).
    /// </summary>
    protected IWebElement FindByText(string text) =>
        App.FindElement(MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{text}\")"));

    /// <summary>
    /// Switches to the named Shell tab by tapping its bottom-bar entry.
    /// Tabs are surfaced as <c>content-desc</c> with the tab Title (e.g.
    /// "Directions", "Traffic"), so we match on accessibility-id rather
    /// than AutomationId. Coordinates are computed from the element bounds
    /// returned by UIAutomator2 — no hard-coded positions.
    /// </summary>
    protected void NavigateToTab(string tabTitle)
    {
        var tab = App.FindElement(MobileBy.AccessibilityId(tabTitle));
        tab.Click();
    }

    /// <summary>
    /// Captures a screenshot named after the running test, useful for
    /// diagnosing failures in CI.
    /// </summary>
    protected void Screenshot(string name)
    {
        var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        App.GetScreenshot().SaveAsFile(Path.Combine(dir, $"{name}.png"));
    }
}
