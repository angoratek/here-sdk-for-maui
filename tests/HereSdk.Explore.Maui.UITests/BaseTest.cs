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
    public void RestoreAppStateAfterTest()
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

        // A test that ends on a pushed page (e.g. Settings) hides the Shell
        // tab bar and contaminates every later fixture — NavigateToTab fails
        // instantly for the rest of the run. Restore a known state: go back
        // once to pop the pushed page, and relaunch the app as a last resort.
        if (TabBarVisible()) return;

        try
        {
            App.Navigate().Back();
        }
        catch
        {
            // Driver may not support Navigate().Back() — fall through.
        }
        if (TabBarVisible()) return;

        try
        {
            // A sleep between terminate and activate: activating while the
            // old process is still dying can hang the new process at
            // startup ("failed to complete startup" ANR).
            App.TerminateApp(AppiumSetup.AppBundleId);
            Thread.Sleep(2000);
            App.ActivateApp(AppiumSetup.AppBundleId);
        }
        catch
        {
            // Ignore — the next test's NavigateToTab will fail with a
            // clear NoSuchElementException either way.
        }
    }

    private bool TabBarVisible()
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (TryFindUIElement("ExploreMapView") is not null ||
                TryFindTab("Explore") is not null)
            {
                return true;
            }
            Thread.Sleep(250);
        }
        return false;
    }

    private IWebElement? TryFindTab(string title)
    {
        try
        {
            return App.FindElement(MobileBy.AccessibilityId(title));
        }
        catch (NoSuchElementException)
        {
            return null;
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
        WaitForElement(AutomationIdSelector(automationId), TimeSpan.FromSeconds(5));

    /// <summary>
    /// Like <see cref="FindUIElement"/> but with an explicit timeout in
    /// seconds — use when an element needs longer than the default 5s
    /// poll window (e.g. a pushed-page navigation on a slow emulator).
    /// </summary>
    protected IWebElement WaitForUIElement(string automationId, int timeoutSeconds) =>
        WaitForElement(AutomationIdSelector(automationId), TimeSpan.FromSeconds(timeoutSeconds));

    /// <summary>
    /// Polls for an element for up to <paramref name="timeout"/> but returns
    /// null instead of throwing — for flows that may legitimately never
    /// produce the element (e.g. no network / no credentials on the runner).
    /// </summary>
    protected IWebElement? TryPollForUIElement(string automationId, TimeSpan timeout)
    {
        try
        {
            return WaitForElement(AutomationIdSelector(automationId), timeout);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    /// <summary>
    /// Like <see cref="FindUIElement"/> but returns null instead of
    /// throwing when the element is not present. Use for conditionally
    /// visible elements (e.g. the place card's CTA, which is only in
    /// the tree when a search has returned results). Does NOT poll —
    /// absence checks must stay instant.
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
        WaitForElement(TextSelector(text), TimeSpan.FromSeconds(5));

    /// <summary>
    /// Locates a UI element whose visible text contains
    /// <paramref name="substring"/>. Cross-platform twin of
    /// <see cref="FindAllContainingText"/> that returns a single match.
    /// </summary>
    protected IWebElement FindByTextContains(string substring) =>
        WaitForElement(ContainsTextSelector(substring), TimeSpan.FromSeconds(5));

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
        // Poll instead of a single lookup: right after a session start or a
        // page transition the tab bar may not be in the accessibility tree
        // yet, and with no implicit wait a single FindElement returns
        // instantly on a miss.
        var tab = WaitForElement(MobileBy.AccessibilityId(tabTitle), TimeSpan.FromSeconds(10));
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

    /// <summary>
    /// Polls for an element every 250ms until the timeout elapses. The
    /// suite runs with no implicit wait (see AppiumSetup), so presence
    /// lookups must poll here to survive page-transition races on slow
    /// emulators — a single FindElement returns instantly on a miss.
    /// </summary>
    private IWebElement WaitForElement(By by, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            try
            {
                return App.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                if (DateTime.UtcNow >= deadline) throw;
                Thread.Sleep(250);
            }
        }
    }

    /// <summary>
    /// Taps the map and waits for the place card to slide up, retrying
    /// the tap once. Handles two races:
    /// 1. A stale card left open by an earlier test (Shell preserves page
    ///    state across tab switches) — the first tap only dismisses it, so
    ///    dismiss up front and wait for the collapse to finish.
    /// 2. The CTA briefly visible while a stale sheet collapses — confirm
    ///    the CTA is still there after a settle pause before declaring the
    ///    card open.
    /// Returns the card's Get Directions CTA, or null if no card appeared.
    /// </summary>
    protected IWebElement? TapMapForPlaceCard(IWebElement map)
    {
        if (TryFindUIElement("PlaceCardDirectionsButton") is not null)
        {
            map.Click();
            var settle = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < settle && TryFindUIElement("PlaceCardDirectionsButton") is not null)
            {
                Thread.Sleep(250);
            }
        }

        for (var attempt = 0; attempt < 2; attempt++)
        {
            map.Click();
            var deadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < deadline)
            {
                if (TryFindUIElement("PlaceCardDirectionsButton") is not null)
                {
                    // Settle: the sheet expansion (or a stale card's
                    // collapse) takes a few hundred ms.
                    Thread.Sleep(1000);
                    var cta = TryFindUIElement("PlaceCardDirectionsButton");
                    if (cta is not null) return cta;
                    break;
                }
                Thread.Sleep(500);
            }
        }
        return null;
    }

    /// <summary>
    /// Hides the on-screen keyboard, tolerating drivers/emulators that
    /// fail HideKeyboard ("The software keyboard cannot be hidden" on
    /// uiautomator2) — falls back to the BACK key, which dismisses the
    /// keyboard first before navigating.
    /// </summary>
    protected void DismissKeyboard()
    {
        try
        {
            App.HideKeyboard();
            return;
        }
        catch
        {
            // HideKeyboard unsupported or the keyboard refused — fall through.
        }
        try
        {
            App.Navigate().Back();
        }
        catch
        {
            // No keyboard to dismiss — nothing to do.
        }
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
