using NUnit.Framework;

using OpenQA.Selenium.Appium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Tests for the Tools page drawing tools and Demo Gallery. Taps each
/// drawing tool button and asserts no error appears. The drawing tools
/// are now visible by default (CurrentState="FullyExpanded" on
/// <c>ToolsSheet</c>) — this test is the regression guard against
/// the page landing in a near-empty collapsed sheet.
/// </summary>
public class ToolsPageDrawingTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToTools() => NavigateToTab("Tools");

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
        // Just verify the page renders with all 4 new ids present.
        // (Tapping a scheme chip is tested in ToolsPageSchemeTests.)
        Assert.That(FindUIElement("ToolsMapView").Displayed, Is.True,
            "ToolsMapView should be present at the top of the page");
        Assert.That(FindUIElement("ToolsMarkerButton").Displayed, Is.True,
            "ToolsMarkerButton should be visible (sheet is fully expanded by default)");
        Assert.That(FindUIElement("ToolsClearAllButton"), Is.Not.Null,
            "ToolsClearAllButton should exist in the visual tree (initially hidden but in the XAML)");
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
        var matches = App.FindElements(
            MobileBy.AndroidUIAutomator($"new UiSelector().textContains(\"{NotInitializedSignature}\")"));
        if (matches.Count > 0)
        {
            var texts = string.Join(" | ", matches.Select(m => m.Text));
            Assert.Fail($"Found {matches.Count} element(s) containing '{NotInitializedSignature}': {texts}");
        }
    }
}
