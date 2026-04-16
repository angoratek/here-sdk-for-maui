using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class TrafficPage : ContentPage
{
    public TrafficPage(TrafficViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}