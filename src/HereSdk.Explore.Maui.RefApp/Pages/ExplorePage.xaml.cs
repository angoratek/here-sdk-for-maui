using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class ExplorePage : ContentPage
{
    private readonly ExploreViewModel _viewModel;

    public ExplorePage(ExploreViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        MapView.HandlerChanged += OnMapViewHandlerChanged;

        CategoryChips.CategorySelected += (_, chip) =>
        {
            _viewModel.SearchCategoryCommand.Execute(chip.CategoryId);
        };

        PlaceCardView.DirectionsClicked += (_, _) =>
        {
            _viewModel.NavigateToDirectionsCommand.Execute(null);
        };

        StylePicker.SchemeChanged += (_, scheme) =>
        {
            _viewModel.ChangeSchemeCommand.Execute(scheme.ToString());
        };
    }

    private void OnMapViewHandlerChanged(object? sender, EventArgs e)
    {
        if (MapView.Handler is not null && MapView.Map is not null)
        {
            MapView.HandlerChanged -= OnMapViewHandlerChanged;
            _viewModel.SetMapService(MapView.Map);
        }
    }
}
