using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;

namespace Here.Explore.Maui.UITests;

/// <summary>
/// Selects which platform the test process drives. Defaults to
/// <see cref="Android"/>; override with the <c>UITEST_PLATFORM</c> env
/// var (e.g. <c>UITEST_PLATFORM=iOS dotnet test ...</c>).
/// </summary>
public enum TestPlatform
{
    Android,
    iOS,
}

/// <summary>
/// NUnit [SetUpFixture] that starts an Appium 2.x server (if one is not already
/// running) and creates a single <see cref="AppiumDriver"/> shared by every
/// test in this assembly. The driver type is picked from
/// <c>UITEST_PLATFORM</c> (default Android), so the same test files can be
/// run on either platform.
///
/// Pattern is taken from the official dotnet/maui-samples UITesting sample.
/// </summary>
[SetUpFixture]
public class AppiumSetup
{
    private static AppiumDriver? _driver;

    public const string AppPackage = "com.here.explore.maui.refapp";
    public const string AppActivity = "crc6416fb3a76ff63a6fe.MainActivity";
    public const string AppBundleId = "com.here.explore.maui.refapp";

    /// <summary>
    /// Platform that the current test process is driving. Resolved once
    /// at <see cref="OneTimeSetUp"/> time from the <c>UITEST_PLATFORM</c>
    /// env var so the rest of the suite can branch on it cheaply.
    /// </summary>
    public static TestPlatform Platform { get; private set; } = TestPlatform.Android;

    public static AppiumDriver App =>
        _driver ?? throw new InvalidOperationException("AppiumDriver is null — was OneTimeSetUp skipped?");

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        // If a CI runner is already hosting Appium on 4723, this is a no-op.
        AppiumServerHelper.StartAppiumLocalServer();

        Platform = ResolvePlatform();

        _driver = Platform switch
        {
            TestPlatform.iOS => CreateiOSDriver(),
            _ => CreateAndroidDriver(),
        };

        // No implicit wait. Per-element lookups (NavigateToTab,
        // FindUIElement) return immediately on a miss. The previous
        // attempt to set ImplicitWait = 5s here was wrong: it gave the
        // iOS 26 WDA session "more time to fail" instead of "more
        // time to succeed", because the RefApp on iOS 26 is slow to
        // first-paint and was never in the tree within 5s. Instead we
        // run a one-shot readiness probe in [OneTimeSetUp] below —
        // `WaitForAppReady()` — that gives the app up to 60s to
        // surface a known element before any test starts. Tests then
        // see a fully-launched app and can fail fast on real misses.
        WaitForAppReady();
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        _driver?.Quit();
        _driver?.Dispose();
        _driver = null;

        AppiumServerHelper.DisposeAppiumLocalServer();
    }

    private static TestPlatform ResolvePlatform()
    {
        var raw = Environment.GetEnvironmentVariable("UITEST_PLATFORM");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return TestPlatform.Android;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "ios" or "iphone" or "ipad" => TestPlatform.iOS,
            "android" => TestPlatform.Android,
            _ => throw new ArgumentException(
                $"Unsupported UITEST_PLATFORM '{raw}'. Use 'android' or 'ios'."),
        };
    }

    private static AndroidDriver CreateAndroidDriver()
    {
        // The app's Android package name is set on the RefApp's csproj
        // (ApplicationId = com.here.explore.maui.refapp). MAUI's Android
        // templates emit an AOT'd MainActivity whose full type name is
        // `crc6416fb3a76ff63a6fe.MainActivity` — discovered via
        // `adb shell cmd package resolve-activity --brief <pkg>`.

        var androidOptions = new AppiumOptions
        {
            // UIAutomator2 is the Android driver, recommended for native + MAUI.
            AutomationName = "UIAutomator2",
            PlatformName = "Android",
        };

        // NoReset=true preserves the debug-keystore app data between runs, so
        // we can iterate without re-installing the APK every time.
        androidOptions.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");
        androidOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, AppPackage);
        androidOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, AppActivity);

        // `avd` is set by the CI workflow (testavd) so the emulator boots
        // automatically. On a local dev box, start your emulator manually and
        // comment this out.
        // androidOptions.AddAdditionalAppiumOption("avd", "testavd");

        // Pass an explicit Uri so the driver connects to the Appium server
        // started by AppiumServerHelper (or one already running externally)
        // instead of spawning its own. Appium.WebDriver 5.x defaults to
        // auto-starting an embedded Appium process when constructed without
        // a Uri, which then fails with EADDRINUSE because the port is
        // already held by the AppiumServerHelper-managed instance.
        return new AndroidDriver(
            new Uri($"http://{AppiumServerHelper.DefaultHostAddress}:{AppiumServerHelper.DefaultHostPort}"),
            androidOptions,
            TimeSpan.FromSeconds(180));
    }

    private static IOSDriver CreateiOSDriver()
    {
        // XCUITest is the Appium driver for iOS. The bundle id is the
        // universal primary key on iOS — there is no equivalent of
        // Android's MainActivity. `noReset=true` parallels the Android
        // option, preserving app data between runs.
        //
        // UDID is optional when the host has exactly one booted sim, but
        // we set it explicitly so a dev box with multiple simulators does
        // not accidentally target the wrong one. Override with the
        // UITEST_IOS_UDID env var.
        var iosOptions = new AppiumOptions
        {
            AutomationName = "XCUITest",
            PlatformName = "iOS",
        };

        iosOptions.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");
        iosOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, AppBundleId);
        iosOptions.AddAdditionalAppiumOption("useNewWDA", "false");
        iosOptions.AddAdditionalAppiumOption("wdaLaunchTimeout", "60000");
        iosOptions.AddAdditionalAppiumOption("wdaConnectionTimeout", "60000");

        var udid = Environment.GetEnvironmentVariable("UITEST_IOS_UDID");
        if (!string.IsNullOrWhiteSpace(udid))
        {
            iosOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
        }

        return new IOSDriver(
            new Uri($"http://{AppiumServerHelper.DefaultHostAddress}:{AppiumServerHelper.DefaultHostPort}"),
            iosOptions,
            TimeSpan.FromSeconds(180));
    }

    /// <summary>
    /// Blocks until the RefApp's main UI is ready to be driven, or
    /// fails the test run with a clear diagnostic. Polls for the
    /// Explore page's <c>ExploreMapView</c> (the first element a test
    /// looks at) once per second for up to 180 seconds. iOS 26 in a
    /// fresh WDA session is visibly slower to first-paint than the
    /// Android emulator, so a single "did the launch screen go
    /// away" check is not enough — the WDA tree may briefly show
    /// a launch screen, the home screen, the dictation alert, and
    /// only then the real UI. On a cold sim the XCUITest driver
    /// also has to compile and install WebDriverAgent, which eats
    /// ~60-90s of the budget before the app tree is even visible.
    /// 180s gives the WDA build + iOS 26 first-paint enough runway
    /// to settle.
    /// </summary>
    private static void WaitForAppReady()
    {
        // The same AutomationId is used for the probe on both
        // platforms — BaseTest.AutomationIdSelector adapts the
        // locator (resource-id on Android, accessibility id on iOS).
        const string probeId = "ExploreMapView";
        var deadline = DateTime.UtcNow.AddSeconds(180);

        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                _driver!.FindElement(ProbeSelector(probeId));
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;
                Thread.Sleep(1000);
            }
        }

        throw new InvalidOperationException(
            $"RefApp did not surface '{probeId}' within 180s of Appium session start. " +
            $"Last probe error: {lastError?.Message}",
            lastError);
    }

    private static By ProbeSelector(string automationId) =>
        Platform switch
        {
            TestPlatform.iOS => MobileBy.AccessibilityId(automationId),
            _ => MobileBy.Id($"{AppPackage}:id/{automationId}"),
        };
}
