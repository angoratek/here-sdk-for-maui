using System.ComponentModel;
using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Panels;

/// <summary>
/// Explore overlays (search, suggestions, place card, style picker) shown
/// over the shared map — the HereMapView itself lives in MapHomePage.
/// </summary>
public partial class ExplorePanel : ContentView
{
    private readonly ExploreViewModel _viewModel;

    public ExplorePanel(ExploreViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

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

        // The place card lives in PlaceSheet, which is collapsed at 0 height.
        // IsPlaceCardVisible only controls visibility — expand/collapse the
        // sheet here so the card actually slides up (and back down on dismiss).
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExploreViewModel.SelectedPlace))
        {
            if (_viewModel.SelectedPlace is { } place)
                PlaceCardView.LoadPlace(place, _viewModel.SelectedPlaceDistanceKm);
        }
        else if (e.PropertyName == nameof(ExploreViewModel.IsPlaceCardVisible))
        {
            PlaceSheet.CurrentState = _viewModel.IsPlaceCardVisible
                ? BottomSheet.SheetState.HalfExpanded
                : BottomSheet.SheetState.Collapsed;
        }
    }
}