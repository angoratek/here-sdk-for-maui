using NUnit.Framework;

using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Tests for the Traffic page: tapping the Flow / Incidents / Refresh
/// buttons and asserting the page does not error. We don't try to
/// assert specific map-rendered objects (Appium can't see them on the
/// MapView) but the "not initialized" error is what we want to catch.
/// </summary>
public class TrafficPageInteractionTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToTraffic() => NavigateToTab("Traffic");

    [Test]
    public void TapRefresh_DoesNotShowServiceNotInitializedError()
    {
        var refresh = FindUIElement("TrafficRefreshButton");
        refresh.Click();

        Screenshot(nameof(TapRefresh_DoesNotShowServiceNotInitializedError));

        System.Threading.Thread.Sleep(5000);
        AssertNoErrorBanner(NotInitializedSignature);
    }

    [Test]
    public void ToggleFlow_DoesNotShowServiceNotInitializedError()
    {
        // The Flow button is already on the page (default state).
        var flow = FindUIElement("TrafficFlowButton");
        flow.Click();
        Screenshot(nameof(ToggleFlow_DoesNotShowServiceNotInitializedError));
        System.Threading.Thread.Sleep(3000);
        AssertNoErrorBanner(NotInitializedSignature);

        // Toggle back to the original state.
        flow.Click();
        Screenshot("ToggleFlow_ToggledBack");
        System.Threading.Thread.Sleep(3000);
        AssertNoErrorBanner(NotInitializedSignature);
    }

    [Test]
    public void ToggleIncidents_DoesNotShowServiceNotInitializedError()
    {
        var incidents = FindUIElement("TrafficIncidentsButton");
        incidents.Click();
        Screenshot(nameof(ToggleIncidents_DoesNotShowServiceNotInitializedError));
        System.Threading.Thread.Sleep(3000);
        AssertNoErrorBanner(NotInitializedSignature);

        incidents.Click();
        Screenshot("ToggleIncidents_ToggledBack");
        System.Threading.Thread.Sleep(3000);
        AssertNoErrorBanner(NotInitializedSignature);
    }

    private void AssertNoErrorBanner(string substring)
    {
        var matches = App.FindElements(
            MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\"{substring}\")"));
        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{substring}': {texts}. " +
                $"A service was used before its Initialize() ran.");
        }
    }
}
