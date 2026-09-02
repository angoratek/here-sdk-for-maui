using NUnit.Framework;

using OpenQA.Selenium;

namespace Here.Explore.Maui.UITests.PageObjects;

/// <summary>
/// End-to-end UI test for the isoline flow. Tapping the isoline toggle
/// must not surface "not initialized" or "not yet supported" — the
/// latter was the standing error on iOS before the isoline engine was
/// added to NativeBridge. After the toggle, the isoline polygon render
/// (or a benign routing error) is accepted; a NotImplementedException
/// banner is not.
/// </summary>
public class DirectionsPageIsolineTests : BaseTest
{
    private const string NotSupportedSignature = "not yet supported";
    private const string NotInitializedSignature = "not initialized";

    [SetUp]
    public void NavigateToDirections() => NavigateToTab("Directions");

    [Test]
    public void ToggleIsoline_DoesNotShowUnsupportedOrNotInitializedError()
    {
        var button = FindUIElement("DirectionsIsolineButton");
        button.Click();

        // Give the isoline calculation a moment to complete or fail.
        System.Threading.Thread.Sleep(8000);

        Screenshot(nameof(ToggleIsoline_DoesNotShowUnsupportedOrNotInitializedError));

        foreach (var signature in new[] { NotSupportedSignature, NotInitializedSignature })
        {
            var matches = FindAllContainingText(signature);
            if (matches.Count > 0)
            {
                var texts = string.Join(" | ", matches.Select(m => m.Text));
                Assert.Fail(
                    $"Found {matches.Count} element(s) containing '{signature}': {texts}.");
            }
        }
    }
}