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
    /// Locates a UI element by its MAUI <c>AutomationId</c>. The maui-samples
    /// BasicAppiumNunitSample wraps this because Windows uses a different
    /// locator strategy; for Android-only tests the direct call is fine.
    /// </summary>
    protected IWebElement FindUIElement(string automationId) =>
        App.FindElement(MobileBy.AccessibilityId(automationId));

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
