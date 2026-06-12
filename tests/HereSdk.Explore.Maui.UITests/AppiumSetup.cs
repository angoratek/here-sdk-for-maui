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

    public static AppiumDriver App =>
        _driver ?? throw new InvalidOperationException("AppiumDriver is null — was OneTimeSetUp skipped?");

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        // If a CI runner is already hosting Appium on 4723, this is a no-op.
        AppiumServerHelper.StartAppiumLocalServer();

        // The app's Android package name is set on the RefApp's csproj
        // (ApplicationId = com.here.explore.maui.refapp). Adjust here if you
        // rename the package.
        const string AppPackage = "com.here.explore.maui.refapp";
        const string AppActivity = "com.here.explore.maui.refapp.MainActivity";

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

        _driver = new AndroidDriver(androidOptions);
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
