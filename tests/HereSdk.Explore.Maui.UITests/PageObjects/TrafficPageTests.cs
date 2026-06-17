using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies the Traffic tab's flow/incidents toggle chips. As with the other
/// pages, real traffic data is not asserted — that needs network + HERE
/// access keys and is best covered by device tests or a live integration job.
/// </summary>
public class TrafficPageTests : BaseTest
{
    [SetUp]
    public void NavigateToTraffic() => NavigateToTab("Traffic");

    [Test]
    public void TrafficMapView_IsPresent()
    {
        Screenshot(nameof(TrafficMapView_IsPresent));

        Assert.That(FindUIElement("TrafficMapView").Displayed, Is.True);
    }

    [Test]
    public void FlowAndIncidentsButtons_ArePresent()
    {
        Screenshot(nameof(FlowAndIncidentsButtons_ArePresent));

        Assert.That(FindUIElement("TrafficFlowButton").Displayed, Is.True);
        Assert.That(FindUIElement("TrafficIncidentsButton").Displayed, Is.True);
    }
}
