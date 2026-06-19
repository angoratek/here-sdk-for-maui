using NUnit.Framework;

using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace Here.Explore.Maui.UITests;

/// <summary>
/// NUnit [SetUpFixture] that starts an Appium 2.x server (if one is not already
/// running) and creates a single <see cref="AndroidDriver"/> shared by every
/// test in this assembly. Pattern is taken from the official dotnet/maui-samples
/// UITesting sample.
/// </summary>
[SetUpFixture]
public class AppiumSetup
{
    private static AppiumDriver? _driver;

    public const string AppPackage = "com.here.explore.maui.refapp";
    public const string AppActivity = "crc6416fb3a76ff63a6fe.MainActivity";

    public static AppiumDriver App =>
        _driver ?? throw new InvalidOperationException("AppiumDriver is null — was OneTimeSetUp skipped?");

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        // If a CI runner is already hosting Appium on 4723, this is a no-op.
        AppiumServerHelper.StartAppiumLocalServer();

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
        _driver = new AndroidDriver(
            new Uri($"http://{AppiumServerHelper.DefaultHostAddress}:{AppiumServerHelper.DefaultHostPort}"),
            androidOptions,
            TimeSpan.FromSeconds(180));
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        _driver?.Quit();
        _driver?.Dispose();
        _driver = null;

        AppiumServerHelper.DisposeAppiumLocalServer();
    }
}
