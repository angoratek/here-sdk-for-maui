using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class MapItemsPage : ContentPage
{
    public MapItemsPage()
    {
        InitializeComponent();
        BindingContext = new MapItemsViewModel();
    }

    public MapItemsPage(MapItemsViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}