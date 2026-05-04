using Here.Explore.Maui.RefApp.Pages;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = activationState?.Context?.Services?.GetService<ModernMainPage>()
            ?? new ModernMainPage(
                new ModernMainViewModel(
                    new Services.SearchService(),
                    new Services.RoutingService(),
                    new Services.LocationService()),
                new SettingsViewModel());

        return new Window(page);
    }
}
