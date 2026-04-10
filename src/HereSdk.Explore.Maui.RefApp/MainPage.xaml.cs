using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.RefApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MapViewModel();
    }
}