using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Verifies the Explore tab is the default landing tab and that the search
/// entry accepts input. Map rendering is intentionally NOT asserted — the
/// swiftshader GPU on CI emulators can fail HERE SDK's EGL initialization,
/// so the test suite focuses on UI navigation only.
/// </summary>
public class ExplorePageTests : BaseTest
{
    [SetUp]
    public void NavigateToExplore() => NavigateToTab("Explore");

    [Test]
    public void AppLaunches_ShowsExploreMapView()
    {
        Screenshot(nameof(AppLaunches_ShowsExploreMapView));

        var map = FindUIElement("ExploreMapView");
        Assert.That(map, Is.Not.Null);
        Assert.That(map.Displayed, Is.True);
    }

    [Test]
    public void SearchEntry_AcceptsText()
    {
        var search = FindUIElement("ExploreSearchEntry");
        search.Clear();
        search.SendKeys("coffee");

        Screenshot(nameof(SearchEntry_AcceptsText));

        Assert.That(search.Text, Is.EqualTo("coffee"));
    }
}
