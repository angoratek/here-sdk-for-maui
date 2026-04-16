using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class RoutingPage : ContentPage
{
    public RoutingPage(RoutingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}