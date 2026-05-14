using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class ExploreViewModelTests
{
    private readonly ISearchService _searchService;
    private readonly ILocationService _locationService;
    private readonly ExploreViewModel _viewModel;

    public ExploreViewModelTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _locationService = Substitute.For<ILocationService>();
        _viewModel = new ExploreViewModel(_searchService, _locationService);
    }

    #region Initial State

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        Assert.Equal("", _viewModel.SearchQuery);
        Assert.False(_viewModel.HasSuggestions);
        Assert.False(_viewModel.IsSearching);
        Assert.False(_viewModel.IsPlaceCardVisible);
        Assert.Null(_viewModel.SelectedPlace);
    }

    #endregion

    #region SearchQuery

    [Fact]
    public async Task SearchQuery_WhenAtLeastTwoChars_TriggersSuggest()
    {
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, new List<Suggestion>()));

        _viewModel.SearchQuery = "SF";

        await Task.Delay(400);

        await _searchService.Received(1).SuggestAsync(
            Arg.Any<TextQuery>(),
            Arg.Is<SearchOptions>(o => o.MaxItems == 6));
    }

    [Fact]
    public void SearchQuery_WithLessThanTwoChars_DoesNotTriggerSuggest()
    {
        _viewModel.SearchQuery = "S";

        Assert.False(_viewModel.HasSuggestions);
    }

    #endregion

    #region SubmitSearch

    [Fact]
    public async Task SubmitSearch_WithValidQuery_SearchesForPlaces()
    {
        _viewModel.SearchQuery = "SF";
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        var place = new Place("id1", "San Francisco", new GeoCoordinates(37.77, -122.42));
        _searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place> { place }));

        _viewModel.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        await _searchService.Received(1).SearchAsync(
            Arg.Any<TextQuery>(),
            Arg.Is<SearchOptions>(o => o.MaxItems == 20));
        mockMap.Received(1).AddMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public void SubmitSearch_WithEmptyQuery_DoesNothing()
    {
        _viewModel.SearchQuery = "";
        _viewModel.SubmitSearchCommand.Execute(null);

        _searchService.DidNotReceive().SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>());
    }

    #endregion

    #region SelectSuggestion

    [Fact]
    public async Task SelectSuggestion_RetrievesPlaceAndShowsOnMap()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        var suggestion = new Suggestion("Golden Gate Bridge", "place1", SuggestionType.Place);
        var place = new Place("place1", "Golden Gate Bridge", new GeoCoordinates(37.8199, -122.4783));
        _searchService.GetPlaceByIdAsync("place1").Returns(place);

        _viewModel.SelectSuggestionCommand.Execute(suggestion);
        await Task.Delay(50);

        Assert.Equal("Golden Gate Bridge", _viewModel.SearchQuery);
        Assert.True(_viewModel.IsPlaceCardVisible);
        Assert.NotNull(_viewModel.SelectedPlace);
        await mockMap.Received(1).SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), 15);
    }

    #endregion

    #region SearchCategory

    [Fact]
    public async Task SearchCategory_SearchesByCategory()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        _searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>()));

        _viewModel.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        await _searchService.Received(1).SearchAsync(
            Arg.Any<CategoryQuery>(),
            Arg.Is<SearchOptions>(o => o.MaxItems == 20));
    }

    #endregion

    #region Zoom

    [Fact]
    public async Task ZoomIn_AdjustsCamera()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.AnimateCameraAsync(Arg.Any<CameraAnimation>()).Returns(Task.CompletedTask);
        _viewModel.SetMapService(mockMap);

        _viewModel.ZoomInCommand.Execute(null);
        await Task.Delay(50);

        await mockMap.Received(1).AnimateCameraAsync(Arg.Is<CameraAnimation>(
            a => a.ZoomLevel > 0 && a.DurationInSeconds < 1));
    }

    [Fact]
    public async Task ZoomOut_AdjustsCamera()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.AnimateCameraAsync(Arg.Any<CameraAnimation>()).Returns(Task.CompletedTask);
        _viewModel.SetMapService(mockMap);

        _viewModel.ZoomOutCommand.Execute(null);
        await Task.Delay(50);

        await mockMap.Received(1).AnimateCameraAsync(Arg.Any<CameraAnimation>());
    }

    #endregion

    #region CenterOnLocation

    [Fact]
    public async Task CenterOnLocation_UsesLocationService()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), Arg.Any<double>()).Returns(Task.CompletedTask);
        mockMap.AddLocationIndicator(Arg.Any<LocationIndicator>());
        _viewModel.SetMapService(mockMap);

        var location = new Models.Location(new GeoCoordinates(37.77, -122.42));
        _locationService.GetCurrentLocationAsync().Returns(location);

        _viewModel.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        await _locationService.Received(1).GetCurrentLocationAsync();
        await mockMap.Received(1).SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), 16);
        mockMap.Received(1).AddLocationIndicator(Arg.Any<LocationIndicator>());
    }

    #endregion

    #region Scheme

    [Fact]
    public async Task ChangeScheme_LoadsScene()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.LoadSceneAsync(Arg.Any<MapScheme>()).Returns(Task.CompletedTask);
        _viewModel.SetMapService(mockMap);

        _viewModel.ChangeSchemeCommand.Execute("SatelliteDay");
        await Task.Delay(50);

        await mockMap.Received(1).LoadSceneAsync(MapScheme.SatelliteDay);
        Assert.Equal(MapScheme.SatelliteDay, _viewModel.CurrentScheme);
    }

    #endregion

    #region ClearSearch

    [Fact]
    public void ClearSearch_ResetsState()
    {
        _viewModel.SearchQuery = "test";
        _viewModel.ClearSearchCommand.Execute(null);

        Assert.Equal("", _viewModel.SearchQuery);
        Assert.False(_viewModel.HasSuggestions);
        Assert.False(_viewModel.IsPlaceCardVisible);
    }

    #endregion

    #region NavigateToDirections

    [Fact]
    public void NavigateToDirections_ExistsAndCanExecute()
    {
        Assert.NotNull(_viewModel.NavigateToDirectionsCommand);
        Assert.True(_viewModel.NavigateToDirectionsCommand.CanExecute(null));
    }

    #endregion

    #region CenterOnLocation_Error

    [Fact]
    public async Task CenterOnLocation_WhenServiceFails_DoesNotCrash()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);
        _locationService.GetCurrentLocationAsync().Returns(Task.FromException<Models.Location?>(new Exception("GPS error")));

        var exception = await Record.ExceptionAsync(async () =>
        {
            _viewModel.CenterOnLocationCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    #endregion
}
