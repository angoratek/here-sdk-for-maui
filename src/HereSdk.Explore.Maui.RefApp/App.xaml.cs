using Here.Explore.Maui;
using Here.Explore.Maui.RefApp.Pages;
using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.Services;

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
        // Create services and ViewModel for the modern main page
        var mapService = new MapService();
        var searchService = new SearchService();
        var routingService = new RoutingService();
        var viewModel = new ModernMainViewModel(mapService, searchService, routingService);
        var page = new ModernMainPage(viewModel);

        var window = new Window(page);
        Android.Util.Log.Debug("REFAPP_DIAG", "CreateWindow returning window");
        return window;
    }
}