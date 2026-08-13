using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end UI test for the Traffic flow. Tapping the Flow button
/// triggers an ISearchService.QueryFlowAsync call. The same DI-wiring
/// regression that broke SearchService and RoutingService affects
/// TrafficService — this test is the catch for that parallel bug.
/// </summary>
public class TrafficPageFlowTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToTraffic() => NavigateToTab("Traffic");

    [Test]
    public void QueryFlow_DoesNotShowServiceNotInitializedError()
    {
        var flowButton = FindUIElement("TrafficFlowButton");
        flowButton.Click();

        Screenshot(nameof(QueryFlow_DoesNotShowServiceNotInitializedError));

        System.Threading.Thread.Sleep(5000);

        var matches = FindAllContainingText(NotInitializedSignature);

        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail(
                $"Found {matches.Count} element(s) containing '{NotInitializedSignature}': {texts}. " +
                $"TrafficService was used before its Initialize() ran.");
        }
    }
}
