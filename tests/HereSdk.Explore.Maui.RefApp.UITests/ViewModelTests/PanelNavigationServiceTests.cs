using Xunit;

using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Services;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class PanelNavigationServiceTests
{
    [Fact]
    public void SwitchTo_DifferentTab_RaisesTabChanged()
    {
        var nav = new PanelNavigationService();
        var tabs = new List<HomeTab>();
        nav.TabChanged += (_, tab) => tabs.Add(tab);

        nav.SwitchTo(HomeTab.Directions);

        Assert.Equal(new[] { HomeTab.Directions }, tabs);
        Assert.Equal(HomeTab.Directions, nav.ActiveTab);
    }

    [Fact]
    public void SwitchTo_SameTab_NoTabChanged()
    {
        var nav = new PanelNavigationService();
        var called = false;
        nav.TabChanged += (_, _) => called = true;

        nav.SwitchTo(HomeTab.Explore); // already the default

        Assert.False(called);
        Assert.Equal(HomeTab.Explore, nav.ActiveTab);
    }

    [Fact]
    public void SwitchTo_BackToExplore_AfterDirectionsTab_RaisesTabChanged()
    {
        // Regression: the place-card CTA used to switch the UI to Directions
        // via ApplyTab directly, leaving ActiveTab stale at Explore — every
        // later SwitchTo(Explore) then early-returned as "same tab" and the
        // Explore panel could never be shown again. The fixed CTA path goes
        // through SwitchTo(Directions), so going back to Explore must fire.
        var nav = new PanelNavigationService();
        var tabs = new List<HomeTab>();
        nav.TabChanged += (_, tab) => tabs.Add(tab);

        nav.SwitchTo(HomeTab.Directions); // what the CTA handler now does
        nav.SwitchTo(HomeTab.Explore);    // what a later Explore-tab tap does

        Assert.Equal(new[] { HomeTab.Directions, HomeTab.Explore }, tabs);
        Assert.Equal(HomeTab.Explore, nav.ActiveTab);
    }
}