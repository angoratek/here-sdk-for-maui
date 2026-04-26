using Here.Explore.Maui.RefApp.Pages;

namespace Here.Explore.Maui.RefApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolve the main page from DI container
        var page = activationState?.Context?.Services?.GetService<ModernMainPage>()
            ?? new ModernMainPage(new ViewModels.ModernMainViewModel(
                new Services.SearchService(),
                new Services.RoutingService(),
                new Services.LocationService()));

        return new Window(page);
    }
}
