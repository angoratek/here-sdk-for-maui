using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end UI test for the Directions route flow. Tapping Calculate
/// after typing origin and destination must not surface
/// "RoutingService not initialized." The same regression that broke
/// search (services registered in DI without Initialize()) affects
/// routing, so this test is the catch for the parallel bug.
/// </summary>
public class DirectionsPageRouteTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToDirections() => NavigateToTab("Directions");

    [Test]
    public void CalculateRoute_DoesNotShowServiceNotInitializedError()
    {
        var from = FindUIElement("DirectionsFromEntry");
        from.Clear();
        from.SendKeys("San Francisco, CA");

        var to = FindUIElement("DirectionsToEntry");
        to.Clear();
        to.SendKeys("Oakland, CA");

        App.HideKeyboard();

        var button = FindUIElement("DirectionsCalculateButton");
        button.Click();

        Screenshot(nameof(CalculateRoute_DoesNotShowServiceNotInitializedError));

        // Give the route calculation a moment to either complete or fail.
        System.Threading.Thread.Sleep(5000);

        var matches = App.FindElements(
            MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\"{NotInitializedSignature}\")"));

        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{NotInitializedSignature}': {texts}. " +
                $"RoutingService was used before its Initialize() ran.");
        }
    }
}
