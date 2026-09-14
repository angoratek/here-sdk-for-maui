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
    public void TrafficPanel_IsPresent()
    {
        Screenshot(nameof(TrafficPanel_IsPresent));

        // One shared map lives behind all panels (ExploreMapView); the
        // Traffic panel is asserted through its toggle chips.
        Assert.That(FindUIElement("TrafficFlowButton").Displayed, Is.True);
    }

    [Test]
    public void FlowAndIncidentsButtons_ArePresent()
    {
        Screenshot(nameof(FlowAndIncidentsButtons_ArePresent));

        Assert.That(FindUIElement("TrafficFlowButton").Displayed, Is.True);
        Assert.That(FindUIElement("TrafficIncidentsButton").Displayed, Is.True);
    }
}
