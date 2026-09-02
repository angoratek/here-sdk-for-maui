using NUnit.Framework;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// Tests for the Tools page map style scheme switcher. Taps each of
/// the 5 scheme chips and asserts the page does not error. The
/// <c>IMapService.LoadSceneAsync</c> call inside the command will
/// hit the network on a real build, so we tolerate "no scheme change"
/// gracefully (the command path is what we are guarding).
/// </summary>
public class ToolsPageSchemeTests : BaseTest
{
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToTools()
    {
        NavigateToTab("Tools");
        ExpandToolsSettings();
    }

    [Test]
    public void TapNormalDayScheme_DoesNotError()
    {
        FindUIElement("ToolsSchemeNormalDay").Click();
        Screenshot(nameof(TapNormalDayScheme_DoesNotError));
        System.Threading.Thread.Sleep(3000);
        AssertNoError();
    }

    [Test]
    public void TapNormalNightScheme_DoesNotError()
    {
        FindUIElement("ToolsSchemeNormalNight").Click();
        Screenshot(nameof(TapNormalNightScheme_DoesNotError));
        System.Threading.Thread.Sleep(3000);
        AssertNoError();
    }

    [Test]
    public void TapHybridDayScheme_DoesNotError()
    {
        FindUIElement("ToolsSchemeHybridDay").Click();
        Screenshot(nameof(TapHybridDayScheme_DoesNotError));
        System.Threading.Thread.Sleep(3000);
        AssertNoError();
    }

    [Test]
    public void TapSatelliteDayScheme_DoesNotError()
    {
        FindUIElement("ToolsSchemeSatelliteDay").Click();
        Screenshot(nameof(TapSatelliteDayScheme_DoesNotError));
        System.Threading.Thread.Sleep(3000);
        AssertNoError();
    }

    [Test]
    public void TapTerrainDayScheme_DoesNotError()
    {
        FindUIElement("ToolsSchemeTerrainDay").Click();
        Screenshot(nameof(TapTerrainDayScheme_DoesNotError));
        System.Threading.Thread.Sleep(3000);
        AssertNoError();
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
