using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class MapItemsPage : ContentPage
{
    public MapItemsPage()
    {
        InitializeComponent();
    }

    public MapItemsPage(MapItemsViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}