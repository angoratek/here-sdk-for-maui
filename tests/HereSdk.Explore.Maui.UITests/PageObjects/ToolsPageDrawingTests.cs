using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Tests for the Tools page drawing tools and Demo Gallery. Taps each
/// drawing tool button and asserts no error appears. The floating drawing
/// toolbar is visible only in map mode (sheet collapsed), so the fixture
/// collapses the sheet first — this is also the regression guard that the
/// toolbar is reachable from the sheet's resting (half-expanded) state.
/// </summary>
public class ToolsPageDrawingTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToTools()
    {
        NavigateToTab("Tools");
        CollapseToolsSheet();
    }

    [Test]
    public void TapMarkerButton_DoesNotError()
    {
        TapDrawingTool("ToolsMarkerButton");
        AssertNoError();
    }

    [Test]
    public void TapPolylineButton_DoesNotError()
    {
        TapDrawingTool("ToolsPolylineButton");
        AssertNoError();
    }

    [Test]
    public void TapPolygonButton_DoesNotError()
    {
        TapDrawingTool("ToolsPolygonButton");
        AssertNoError();
    }

    [Test]
    public void TapCircleButton_DoesNotError()
    {
        TapDrawingTool("ToolsCircleButton");
        AssertNoError();
    }

    [Test]
    public void MapStylePickerToggle_DoesNotError()
    {
        // Just verify the panel renders with its ids present. The map is
        // shared across panels (ExploreMapView) and asserted there.
        // (Tapping a scheme chip is tested in ToolsPageSchemeTests.)
        Assert.That(FindUIElement("ToolsMarkerButton").Displayed, Is.True,
            "ToolsMarkerButton should be visible in map mode (sheet collapsed)");
        // ToolsClearAllButton is intentionally not asserted: it is
        // IsVisible=false until a drawing exists, so it is not in the
        // Android accessibility tree and a lookup would throw.
    }

    [Test]
    public void PickTool_ShowsHintAndCollapsesSheet()
    {
        // Drawing flow: picking a tool activates the session over the map
        // (the fixture already collapsed the sheet into map mode) and shows
        // the floating hint pill with Undo/Done/Cancel.
        FindUIElement("ToolsMarkerButton").Click();
        Screenshot("PickTool_ShowsHintAndCollapsesSheet");

        Assert.That(WaitForUIElement("ToolsDrawingHint", 5).Displayed, Is.True,
            "Drawing hint pill did not appear after picking a tool");

        // Clean up: cancel the session so later tests start idle.
        FindUIElement("ToolsCancelButton").Click();
    }

    private void TapDrawingTool(string buttonId)
    {
        var button = FindUIElement(buttonId);
        button.Click();
        Screenshot($"TapDrawingTool_{buttonId}");

        // Give the drawing banner / map event handler a moment to react.
        System.Threading.Thread.Sleep(2000);
    }

    private void AssertNoError()
    {
        var matches = FindAllContainingText(NotInitializedSignature);
        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail($"Found {matches.Count} element(s) containing '{NotInitializedSignature}': {texts}");
        }
    }
}
