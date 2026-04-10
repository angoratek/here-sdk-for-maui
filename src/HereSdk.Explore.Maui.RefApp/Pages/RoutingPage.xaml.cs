using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class RoutingPage : ContentPage
{
    public RoutingPage()
    {
        InitializeComponent();
        BindingContext = new RoutingViewModel();
    }

    public RoutingPage(ViewModels.RoutingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}