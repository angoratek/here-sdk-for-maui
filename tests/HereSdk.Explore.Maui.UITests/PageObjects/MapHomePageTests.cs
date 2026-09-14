using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Regression guard for the single shared map: objects drawn on the Tools
/// panel must survive tab switches (every panel draws over the same
/// HereMapView), and "Reset Map" must clear drawing objects, the route,
/// traffic overlays and the search state at once.
/// </summary>
public class MapHomePageTests : BaseTest
{
    [SetUp]
    public void NavigateToTools()
    {
        NavigateToTab("Tools");
        // The floating toolbar (marker/reset buttons) is only visible in map
        // mode — collapse the sheet so the tools are tappable.
        CollapseToolsSheet();
    }

    [Test]
    public void SharedMap_DrawMarker_SwitchTabs_ObjectPersists()
    {
        // Place a marker: pick the tool, then tap the shared map at a
        // point above the fully expanded tools sheet — the map element's
        // center is covered by the sheet, so an element click would hit
        // the sheet instead of the map.
        FindUIElement("ToolsMarkerButton").Click();
        TapMapAboveSheet();

        Screenshot(nameof(SharedMap_DrawMarker_SwitchTabs_ObjectPersists));

        // The object-count badge confirms the marker landed.
        Assert.That(FindByTextContains("objects on map").Displayed, Is.True,
            "Marker was not placed — the object count badge did not appear");

        // Round-trip through another tab and back: the marker must still
        // be on the shared map (badge still shows the object count).
        NavigateToTab("Explore");
        NavigateToTab("Tools");

        Assert.That(FindByTextContains("objects on map").Displayed, Is.True,
            "Drawing did not survive the tab switch — panels appear to draw on separate maps");
    }

    [Test]
    public void ResetMap_ClearsDrawings()
    {
        FindUIElement("ToolsMarkerButton").Click();
        TapMapAboveSheet();
        Assert.That(FindByTextContains("objects on map").Displayed, Is.True,
            "Marker was not placed — the object count badge did not appear");

        FindUIElement("ToolsResetMapButton").Click();

        // The badge disappears once TotalObjectCount returns to 0; poll
        // briefly because the clear runs through the map service.
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline && TryFindUIElement("ToolsClearAllButton") is not null)
        {
            Thread.Sleep(250);
        }

        Screenshot(nameof(ResetMap_ClearsDrawings));

        Assert.That(TryFindUIElement("ToolsClearAllButton"), Is.Null,
            "Reset Map did not clear the drawn objects — badge still visible");
    }
}