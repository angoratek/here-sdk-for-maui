using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Panels;

/// <summary>
/// Traffic overlays (flow/incident toggles, legend, incident sheet) shown
/// over the shared map — the HereMapView itself lives in MapHomePage.
/// </summary>
public partial class TrafficPanel : ContentView
{
    public TrafficPanel(TrafficViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}