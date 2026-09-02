using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests;

/// <summary>
/// Base class for all page-object tests. Exposes the shared <see cref="App"/>
/// (an <see cref="AppiumDriver"/> — Android or iOS) and locator helpers that
/// adapt to the platform the test process is driving (see
/// <see cref="AppiumSetup.Platform"/>).
/// </summary>
public abstract class BaseTest
{    protected AppiumDriver App => AppiumSetup.App;

    [TearDown]
    public void DismissKeyboardAfterTest()
    {
        // Typing tests leave the on-screen keyboard (and on iOS 26 the
        // autocomplete/suggestion overlay, which covers the whole page
        // including the Shell tab bar) open. Every later test then fails
        // with NoSuchElementException. Dismiss it after each test so
        // state never leaks across fixtures.
        try
        {
            App.HideKeyboard();
        }
        catch
        {
            // No keyboard, or the driver cannot hide it — fall through.
        }

        try
        {
            // iOS input accessory bar's Done button — HideKeyboard alone can
            // leave the accessory suggestion strip in the tree on iOS 26.
            App.FindElement(MobileBy.AccessibilityId("Done")).Click();
        }
        catch
        {
            // No Done button (Android, or keyboard already dismissed).
        }
    }

    /// <summary>
    /// Locates a UI element by its MAUI <c>AutomationId</c>.
    /// <list type="bullet">
    ///   <item>On Android, MAUI maps <c>AutomationId</c> to the native
    ///   <c>resource-id</c> (e.g. <c>com.here.explore.maui.refapp:id/ExploreMapView</c>),
    ///   so we use <see cref="MobileBy.Id"/> rather than
    ///   <see cref="MobileBy.AccessibilityId"/>. The accessibility-id
    ///   locator would match against <c>content-desc</c>, which MAUI leaves
    ///   for semantic labels like "Interactive map for exploring places".</item>
    ///   <item>On iOS, MAUI maps <c>AutomationId</c> to the native
    ///   <c>accessibility identifier</c>, which XCUITest exposes via the
    ///   <c>name</c> selector. Appium surfaces that as
    ///   <see cref="MobileBy.AccessibilityId"/>, so iOS uses it directly.</item>
    /// </list>
    /// </summary>
    protected IWebElement FindUIElement(string automationId) =>
        App.FindElement(AutomationIdSelector(automationId));

    /// <summary>
    /// Like <see cref="FindUIElement"/> but returns null instead of
    /// throwing when the element is not present. Use for conditionally
    /// visible elements (e.g. the place card's CTA, which is only in
    /// the tree when a search has returned results).
    /// </summary>
    protected IWebElement? TryFindUIElement(string automationId)
    {
        try
        {
            return App.FindElement(AutomationIdSelector(automationId));
        }
        catch (OpenQA.Selenium.NoSuchElementException)
        {
            return null;
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// Locates a UI element by its visible text. Android uses a
    /// UiAutomator2 <c>UiSelector.text</c>; iOS uses an
    /// <c>NSPredicate</c> matching on the <c>label</c> (which XCUITest
    /// treats as the visible text for <c>UILabel</c>s and
    /// <c>UIButton</c>s).
    /// </summary>
    protected IWebElement FindByText(string text) =>
        App.FindElement(TextSelector(text));

    /// <summary>
    /// Locates a UI element whose visible text contains
    /// <paramref name="substring"/>. Cross-platform twin of
    /// <see cref="FindAllContainingText"/> that returns a single match.
    /// </summary>
    protected IWebElement FindByTextContains(string substring) =>
        App.FindElement(ContainsTextSelector(substring));

    /// <summary>
    /// Expands the collapsed "Settings" section on the Tools page
    /// (tap on the section header) when it is not already open. The
    /// scheme chips and the "More Settings →" button only exist in
    /// the accessibility tree while the section is expanded, so tests
    /// that target them must call this first.
    /// </summary>
    protected void ExpandToolsSettings()
    {
        if (TryFindUIElement("ToolsSchemeNormalDay") is not null)
        {
            return; // already expanded
        }

        FindByText("Settings").Click();
    }

    /// <summary>
    /// Switches to the named Shell tab by tapping its bottom-bar entry.
    /// Tabs are surfaced as <c>content-desc</c> (Android) /
    /// <c>accessibility identifier</c> (iOS) with the tab Title (e.g.
    /// "Directions", "Traffic"), so <see cref="MobileBy.AccessibilityId"/>
    /// matches on both platforms.
    /// </summary>
    protected void NavigateToTab(string tabTitle)
    {
        var tab = App.FindElement(MobileBy.AccessibilityId(tabTitle));
        tab.Click();
    }

    /// <summary>
    /// Returns every element whose visible text contains
    /// <paramref name="substring"/>. Cross-platform: Android uses
    /// UiAutomator2's <c>textContains</c>; iOS uses an
    /// <c>NSPredicate</c> with <c>label CONTAINS[c]</c> (case-insensitive)
    /// so the guard string matches regardless of the platform's
    /// capitalization conventions.
    /// </summary>
    protected System.Collections.ObjectModel.ReadOnlyCollection<OpenQA.Selenium.Appium.AppiumElement> FindAllContainingText(string substring)
    {
        var by = AppiumSetup.Platform switch
        {
            TestPlatform.iOS => new ByIosNSPredicate($"label CONTAINS[c] \"{substring}\""),
            _ => MobileBy.AndroidUIAutomator(
                $"new UiSelector().textContains(\"{substring}\")"),
        };
        return App.FindElements(by);
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

    private static By AutomationIdSelector(string automationId) =>
        AppiumSetup.Platform switch
        {
            TestPlatform.iOS => MobileBy.AccessibilityId(automationId),
            _ => MobileBy.Id($"{AppiumSetup.AppPackage}:id/{automationId}"),
        };

    private static By TextSelector(string text) =>
        AppiumSetup.Platform switch
        {
            TestPlatform.iOS => new ByIosNSPredicate($"label == \"{text}\""),
            _ => MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{text}\")"),
        };

    private static By ContainsTextSelector(string substring) =>
        AppiumSetup.Platform switch
        {
            TestPlatform.iOS => new ByIosNSPredicate($"label CONTAINS[c] \"{substring}\""),
            _ => MobileBy.AndroidUIAutomator(
                $"new UiSelector().textContains(\"{substring}\")"),
        };
}
