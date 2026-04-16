using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class MapItemsPage : ContentPage
{
    public MapItemsPage()
    {
        InitializeComponent();
        BindingContext = new MapItemsViewModel();
    }

    /// <summary>
    /// Sets the map service after the page is created, allowing the ViewModel
    /// to interact with the map from the main page's HereMapView.
    /// </summary>
    internal void SetMapService(Services.IMapService mapService)
    {
        if (BindingContext is MapItemsViewModel vm)
            vm.SetMapService(mapService);
    }
}