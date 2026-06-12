using OpenQA.Selenium.Appium.Service;

namespace Here.Explore.Maui.UITests;

/// <summary>
/// Starts and stops a local Appium 2.x server on 127.0.0.1:4723 for the test
/// process. Tests that need a real device/emulator should not start their own
/// server — they should rely on the consuming SetUpFixture having already
/// done so.
/// </summary>
public static class AppiumServerHelper
{
    private static AppiumLocalService? _service;

    public const string DefaultHostAddress = "127.0.0.1";
    public const int DefaultHostPort = 4723;

    public static void StartAppiumLocalServer(string host = DefaultHostAddress, int port = DefaultHostPort)
    {
        if (_service is not null)
        {
            return;
        }

        var builder = new AppiumServiceBuilder()
            .WithIPAddress(host)
            .UsingPort(port);

        _service = builder.Build();
        _service.Start();
    }

    public static void DisposeAppiumLocalServer()
    {
        _service?.Dispose();
        _service = null;
    }
}
