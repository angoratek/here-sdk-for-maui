using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class TrafficPage : ContentPage
{
    public TrafficPage()
    {
        InitializeComponent();
        BindingContext = new TrafficViewModel();
    }

    public TrafficPage(ViewModels.TrafficViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}