using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;

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
        // A failed test leaves no trace of what the screen actually looked
        // like (most failures throw before the test's own Screenshot call).
        // Capture one before any state restoration so CI/local debugging
        // shows the exact UI at failure time.
        if (TestContext.CurrentContext.Result.Outcome == NUnit.Framework.Interfaces.ResultState.Failure ||
            TestContext.CurrentContext.Result.Outcome == NUnit.Framework.Interfaces.ResultState.Error)
        {
            try
            {
                Screenshot("FAIL_" + TestContext.CurrentContext.Test.Name);
            }
            catch
            {
                // Screenshot is best-effort — never mask the real failure.
            }
        }

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

        // A drawing session left active (a tool was picked but Done/✕ never
        // tapped) keeps the Tools sheet collapsed for every later fixture:
        // the Settings section is hidden and ExpandToolsSettings' swipes
        // land on the map instead. Cancel the session so the sheet restores.
        // The ✕ button only exists in the tree while a session is active,
        // so idle tests pay a single fast lookup.
        try
        {
            TryFindUIElement("ToolsCancelButton")?.Click();
        }
        catch
        {
            // Best-effort — a stuck session surfaces in the next test's
            // failure with a clear screenshot either way.
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

        // uiautomator2 sometimes refuses HideKeyboard ("The software keyboard
        // cannot be hidden"). A keyboard left open covers the shared tab bar,
        // so the next test's NavigateToTab tap lands on the keyboard and the
        // tab never switches. BACK dismisses the keyboard first — only press
        // it when the keyboard is actually up, or it would pop the page.
        try
        {
            if (App.IsKeyboardShown())
            {
                App.Navigate().Back();
            }
        }
        catch
        {
            // Best-effort — the next test's NavigateToTab will fail with a
            // clear NoSuchElementException either way.
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
                TryLocateTab("Explore") is not null)
            {
                return true;
            }
            Thread.Sleep(250);
        }
        return false;
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
    /// Reads an element's text with a stale-element retry: a native view
    /// can be recreated between <c>FindElement</c> and <c>.Text</c> (e.g.
    /// the Directions panel's sheet is still settling its layout after a
    /// tab switch), which surfaces as
    /// <see cref="StaleElementReferenceException"/>. Re-finds and re-reads
    /// until the deadline, then rethrows the last stale error.
    /// </summary>
    protected string GetTextStaleSafe(string automationId, int timeoutSeconds)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (true)
        {
            var element = WaitForUIElement(automationId, timeoutSeconds);
            try
            {
                return element.Text;
            }
            catch (StaleElementReferenceException) when (DateTime.UtcNow < deadline)
            {
                // Native view recreated mid-read — poll for the fresh one.
            }
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
    /// Polls for an element by visible text but returns null instead of
    /// throwing — for sections that may be scrolled out of the sheet's
    /// viewport (off-screen content is not in the accessibility tree).
    /// </summary>
    protected IWebElement? TryFindByText(string text)
    {
        try
        {
            return WaitForElement(TextSelector(text), TimeSpan.FromSeconds(1));
        }
        catch (NoSuchElementException)
        {
            return null;
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// W3C touch swipe upward over the lower half of the screen — scrolls
    /// the tools bottom sheet's content. Swipes (not TapGestureRecognizers
    /// or element clicks) because the sheet's ScrollView is the target.
    /// </summary>
    protected void SwipeSheetUp() => Swipe((int)(App.Manage().Window.Size.Width / 2),
        (int)(App.Manage().Window.Size.Height * 0.75),
        (int)(App.Manage().Window.Size.Width / 2),
        (int)(App.Manage().Window.Size.Height * 0.35));

    /// <summary>
    /// W3C vertical touch swipe between two absolute viewport points.
    /// The Tools bottom sheet snaps state only from a drag that starts on
    /// its header row — the content ScrollView otherwise consumes the
    /// gesture — so sheet-state drags start on the header element.
    /// </summary>
    private void Swipe(int startX, int startY, int endX, int endY)
    {
        var finger = new PointerInputDevice(PointerKind.Touch);
        var sequence = new ActionSequence(finger);
        sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
        sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
        sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, endX, endY, TimeSpan.FromMilliseconds(600)));
        sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));
        App.PerformActions(new[] { sequence });
        Thread.Sleep(500);
    }

    /// <summary>
    /// Drags vertically from the center of the Tools sheet's header row
    /// ("Tools &amp; Settings" — visible in every sheet state). Downward
    /// drags snap Collapsed-ward, upward draps Half/Fully-ward.
    /// </summary>
    private bool DragToolsSheetHeader(int deltaY)
    {
        var header = TryFindByText("Tools & Settings");
        if (header is null)
        {
            return false;
        }

        var x = header.Location.X + header.Size.Width / 2;
        var y = header.Location.Y + header.Size.Height / 2;
        Swipe(x, y, x, y + deltaY);
        return true;
    }

    /// <summary>
    /// Collapses the Tools bottom sheet (map mode: the floating drawing
    /// toolbar becomes visible). Each downward header drag snaps one state
    /// toward Collapsed (FullyExpanded → HalfExpanded → Collapsed); polls
    /// for the toolbar as confirmation.
    /// </summary>
    protected void CollapseToolsSheet()
    {
        for (var attempt = 0; attempt < 4 && TryFindUIElement("ToolsMarkerButton") is null; attempt++)
        {
            DragToolsSheetHeader(+600);
        }
    }

    /// <summary>
    /// Ensures the Tools bottom sheet is not collapsed (expanded enough for
    /// ExpandToolsSettings to scroll its content). An upward header drag
    /// snaps the sheet toward Half/Fully-expanded.
    /// </summary>
    protected void ExpandToolsSheet()
    {
        for (var attempt = 0; attempt < 3 && TryFindUIElement("ToolsMarkerButton") is not null; attempt++)
        {
            DragToolsSheetHeader(-600);
        }
    }

    /// <summary>
    /// W3C touch tap at absolute screen coordinates. Used on the shared
    /// map from the Tools tab, where the fully expanded bottom sheet
    /// covers the map element's center — an element click would hit the
    /// sheet instead of the map.
    /// </summary>
    protected void TapScreen(int x, int y)
    {
        var finger = new PointerInputDevice(PointerKind.Touch);
        var sequence = new ActionSequence(finger);
        sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
        sequence.AddAction(finger.CreatePointerDown(MouseButton.Left));
        sequence.AddAction(finger.CreatePointerUp(MouseButton.Left));
        App.PerformActions(new[] { sequence });
    }

    /// <summary>
    /// Taps the shared map at a point that is visible above the fully
    /// expanded tools sheet (upper ~20% of the screen), so the tap lands
    /// on the map and not on the sheet.
    /// </summary>
    protected void TapMapAboveSheet()
    {
        var size = App.Manage().Window.Size;
        TapScreen(size.Width / 2, (int)(size.Height * 0.2));
    }

    /// <summary>
    /// Expands the collapsed "Settings" section on the Tools panel (tap
    /// on the section header) when it is not already open. The scheme
    /// chips and the "More Settings →" button only exist in the
    /// accessibility tree while the section is expanded — and the whole
    /// section sits below the Demo Gallery cards, off the bottom of the
    /// fully expanded sheet, so it must first be scrolled into view.
    /// Leaves the section scrolled so its controls are tappable.
    /// </summary>
    protected void ExpandToolsSettings()
    {
        if (TryFindUIElement("ToolsSchemeNormalDay") is not null)
        {
            return; // already expanded and in view
        }

        // A prior test may have left the sheet collapsed (map mode, floating
        // toolbar shown) — its content is not scrollable from that state.
        ExpandToolsSheet();

        // Scroll until the Settings header is on screen (or the chips
        // appear, if a prior test already expanded the section).
        IWebElement? header = null;
        for (var attempt = 0; attempt < 6; attempt++)
        {
            header = TryFindByText("Settings");
            if (header is not null || TryFindUIElement("ToolsSchemeNormalDay") is not null)
            {
                break;
            }
            SwipeSheetUp();
        }

        if (TryFindUIElement("ToolsSchemeNormalDay") is null)
        {
            (header ?? FindByText("Settings")).Click();
        }

        // The expanded section (scheme chips, dark-mode toggle) renders
        // below the header — scroll until the chips are on screen. If the
        // chips never show, the tap above collapsed an already-expanded
        // section (view-model state persists across tests), so re-tap.
        var reTapAt = DateTime.UtcNow.AddSeconds(5);
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (TryFindUIElement("ToolsSchemeNormalDay") is null && DateTime.UtcNow < deadline)
        {
            SwipeSheetUp();
            if (DateTime.UtcNow >= reTapAt)
            {
                TryFindByText("Settings")?.Click();
                reTapAt = DateTime.UtcNow.AddSeconds(5);
            }
        }

        // SettingsPageNavigationTests tap ToolsMoreSettingsButton, which sits
        // at the very bottom of the expanded Settings section — below the
        // fold while the sheet is only half-expanded (340dp can't show both
        // the scheme chips and the section bottom). Drag the header up hard:
        // the drag clamps at the fully-expanded position, so the sheet snaps
        // to FullyExpanded (640dp) where the whole section is visible.
        for (var attempt = 0; attempt < 4 && TryFindUIElement("ToolsMoreSettingsButton") is null; attempt++)
        {
            if (!DragToolsSheetHeader(-1500))
            {
                break; // sheet header gone — nothing left to drag
            }
            SwipeSheetUp();
        }
    }

    /// <summary>
    /// Switches to the named tab by tapping its bottom-bar entry on the
    /// single-map home page. The custom tab bar buttons carry the
    /// <c>AutomationId</c> "Tab-{title}" (e.g. "Tab-Directions"); the
    /// legacy Shell <c>content-desc</c> / accessibility-identifier form
    /// is kept as a fallback.
    /// </summary>
    protected void NavigateToTab(string tabTitle)
    {
        // Poll instead of a single lookup: right after a session start or a
        // page transition the tab bar may not be in the accessibility tree
        // yet, and with no implicit wait a single FindElement returns
        // instantly on a miss.
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (true)
        {
            var tab = TryLocateTab(tabTitle);
            if (tab is not null)
            {
                tab.Click();
                return;
            }
            if (DateTime.UtcNow >= deadline)
            {
                throw new OpenQA.Selenium.NoSuchElementException(
                    $"Tab '{tabTitle}' not found in the accessibility tree after 10s");
            }
            Thread.Sleep(250);
        }
    }

    private IWebElement? TryLocateTab(string title)
    {
        foreach (var by in new[] { AutomationIdSelector($"Tab-{title}"), MobileBy.AccessibilityId(title) })
        {
            try
            {
                return App.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                // Try the next selector.
            }
        }
        return null;
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
