using Here.Explore.Maui;

namespace Here.Explore.Maui.RefApp;

public partial class App : Application
{
    public App()
    {
        Android.Util.Log.Debug("REFAPP_DIAG", "App constructor called");
        InitializeComponent();
        Android.Util.Log.Debug("REFAPP_DIAG", "App constructor completed");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Android.Util.Log.Debug("REFAPP_DIAG", $"CreateWindow called, activationState={activationState?.GetType().Name ?? "null"}");
#if ANDROID
        Android.Util.Log.Wtf("REFAPP_DIAG", "App.CreateWindow() REACHED");
#endif
        var window = new Window(new MainPage());
        Android.Util.Log.Debug("REFAPP_DIAG", "CreateWindow returning window");
        return window;
    }
}