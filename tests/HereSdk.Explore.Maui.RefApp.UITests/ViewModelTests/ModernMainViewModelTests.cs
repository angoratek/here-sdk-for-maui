using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

/// <summary>
/// Tests for the ModernMainViewModel commands and properties.
/// These tests verify that all UI buttons in the reference app are properly wired to working commands.
/// </summary>
public class ModernMainViewModelTests : IDisposable
{
    private readonly ISearchService _searchService;
    private readonly IRoutingService _routingService;
    private readonly ILocationService _locationService;
    private readonly ModernMainViewModel _viewModel;

    public ModernMainViewModelTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _routingService = Substitute.For<IRoutingService>();
        _locationService = Substitute.For<ILocationService>();
        _viewModel = new ModernMainViewModel(_searchService, _routingService, _locationService);
    }

    #region Search Tests

    [Fact]
    public void SearchOriginCommand_WithEmptyQuery_DoesNotExecuteSearch()
    {
        // Arrange
        _viewModel.OriginQuery = string.Empty;

        // Act
        _viewModel.SearchOriginCommand.Execute(null);

        // Assert
        _searchService.DidNotReceiveWithAnyArgs().SuggestAsync(default!, default!);
    }

    [Fact]
    public async Task SearchOriginCommand_WithValidQuery_PopulatesOriginSuggestions()
    {
        // Arrange
        _viewModel.OriginQuery = "Berlin";
        var suggestions = new List<Suggestion>
        {
            new("Berlin, Germany", "id1", SuggestionType.Place),
            new("Berlin Street", "id2", SuggestionType.Place),
        };
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, suggestions));

        // Act
        _viewModel.SearchOriginCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        await _searchService.Received(1).SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>());
        Assert.NotNull(_viewModel.OriginSuggestions);
        Assert.Equal(2, _viewModel.OriginSuggestions.Count);
    }

    [Fact]
    public async Task SearchDestinationCommand_WithValidQuery_PopulatesDestinationSuggestions()
    {
        // Arrange
        _viewModel.DestinationQuery = "Munich";
        var suggestions = new List<Suggestion>
        {
            new("Munich, Germany", "id3", SuggestionType.Place),
        };
        _searchService.SuggestAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SuggestResult(SearchError.None, suggestions));

        // Act
        _viewModel.SearchDestinationCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        Assert.NotNull(_viewModel.DestinationSuggestions);
        Assert.Single(_viewModel.DestinationSuggestions);
    }

    [Fact]
    public async Task SelectOriginCommand_SelectsSuggestion()
    {
        // Arrange
        var suggestion = new Suggestion("Berlin, Germany", "id1", SuggestionType.Place);
        var place = new Place("id1", "Berlin, Germany", new GeoCoordinates(52.52, 13.405));
        _searchService.GetPlaceByIdAsync("id1").Returns(place);

        // Act
        _viewModel.SelectOriginCommand.Execute(suggestion);
        await Task.Delay(50);

        // Assert
        Assert.Equal("Berlin, Germany", _viewModel.OriginQuery);
        Assert.Null(_viewModel.OriginSuggestions);
        Assert.NotNull(_viewModel.OriginPlace);
    }

    [Fact]
    public void ClearOriginCommand_ClearsOriginQuery()
    {
        // Arrange
        _viewModel.OriginQuery = "Test Location";

        // Act
        _viewModel.ClearOriginCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, _viewModel.OriginQuery);
    }

    [Fact]
    public void ClearDestinationCommand_ClearsDestinationQuery()
    {
        // Arrange
        _viewModel.DestinationQuery = "Test Destination";

        // Act
        _viewModel.ClearDestinationCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, _viewModel.DestinationQuery);
    }

    [Fact]
    public void SwapLocationsCommand_SwapsLocations()
    {
        // Arrange
        _viewModel.OriginQuery = "Origin";
        _viewModel.DestinationQuery = "Destination";

        // Act
        _viewModel.SwapLocationsCommand.Execute(null);

        // Assert
        Assert.Equal("Destination", _viewModel.OriginQuery);
        Assert.Equal("Origin", _viewModel.DestinationQuery);
    }

    #endregion

    #region Route Tests

    [Fact]
    public void CanCalculateRoute_WhenBothPlacesAreNull_IsFalse()
    {
        Assert.False(_viewModel.CanCalculateRoute);
    }

    [Fact]
    public async Task CalculateRouteCommand_CalculatesRoute()
    {
        // Arrange - set up places
        var originField = typeof(ModernMainViewModel).GetField("_originPlace",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var destField = typeof(ModernMainViewModel).GetField("_destinationPlace",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        originField.SetValue(_viewModel, new Place("id1", "Origin", new GeoCoordinates(52.52, 13.405)));
        destField.SetValue(_viewModel, new Place("id2", "Destination", new GeoCoordinates(48.135, 11.582)));

        var route = new Route(
            "test-route",
            new List<Section>
            {
                new(0, new GeoCoordinates(0, 0), new GeoCoordinates(0, 0), new List<Maneuver>(), SectionTransportMode.Car, 0, 0)
            },
            584000,
            19800
        );
        _routingService.CalculateRouteAsync(Arg.Any<List<Waypoint>>(), Arg.Any<RoutingOptions>())
            .Returns(new RoutingResult(RoutingError.None, new List<Route> { route }));

        // Act
        _viewModel.CalculateRouteCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        Assert.NotNull(_viewModel.CurrentRoute);
        Assert.True(_viewModel.HasRoute);
        Assert.Contains("km", _viewModel.DistanceText);
        Assert.Contains("min", _viewModel.DurationText);
    }

    [Fact]
    public void ClearRouteCommand_ClearsRouteState()
    {
        // Act
        _viewModel.ClearRouteCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.HasRoute);
        Assert.Equal("--", _viewModel.DistanceText);
        Assert.Equal("--", _viewModel.DurationText);
    }

    #endregion

    #region Map Control Tests

    [Fact]
    public void ZoomInCommand_Exists()
    {
        Assert.NotNull(_viewModel.ZoomInCommand);
        Assert.True(_viewModel.ZoomInCommand.CanExecute(null));
    }

    [Fact]
    public void ZoomOutCommand_Exists()
    {
        Assert.NotNull(_viewModel.ZoomOutCommand);
        Assert.True(_viewModel.ZoomOutCommand.CanExecute(null));
    }

    [Fact]
    public void ResetMapOrientationCommand_Exists()
    {
        Assert.NotNull(_viewModel.ResetMapOrientationCommand);
        Assert.True(_viewModel.ResetMapOrientationCommand.CanExecute(null));
    }

    [Fact]
    public void CenterOnLocationCommand_Exists()
    {
        Assert.NotNull(_viewModel.CenterOnLocationCommand);
        Assert.True(_viewModel.CenterOnLocationCommand.CanExecute(null));
    }

    [Fact]
    public async Task CenterOnLocationCommand_UsesLocationService()
    {
        // Arrange
        var location = new Models.Location(new GeoCoordinates(52.52, 13.405));
        _locationService.GetCurrentLocationAsync().Returns(Task.FromResult<Models.Location?>(location));

        // Mock MapService via reflection
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), 15).Returns(Task.CompletedTask);
        mockMapService.AddMapMarker(Arg.Any<MapMarker>());
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        await _locationService.Received(1).GetCurrentLocationAsync();
        await mockMapService.Received(1).SetCameraTargetAsync(Arg.Any<GeoCoordinates>(), 15);
        mockMapService.Received(1).AddMapMarker(Arg.Any<MapMarker>());
    }

    #endregion

    #region Map Objects Tests

    [Fact]
    public void ToggleMapObjectsPanelCommand_TogglesVisibility()
    {
        Assert.False(_viewModel.IsMapObjectsPanelVisible);

        _viewModel.ToggleMapObjectsPanelCommand.Execute(null);
        Assert.True(_viewModel.IsMapObjectsPanelVisible);

        _viewModel.ToggleMapObjectsPanelCommand.Execute(null);
        Assert.False(_viewModel.IsMapObjectsPanelVisible);
    }

    [Fact]
    public void ToggleMarkersCommand_Exists() => Assert.NotNull(_viewModel.ToggleMarkersCommand);

    [Fact]
    public void ToggleCirclesCommand_Exists() => Assert.NotNull(_viewModel.ToggleCirclesCommand);

    [Fact]
    public void TogglePolylinesCommand_Exists() => Assert.NotNull(_viewModel.TogglePolylinesCommand);

    [Fact]
    public void TogglePolygonsCommand_Exists() => Assert.NotNull(_viewModel.TogglePolygonsCommand);

    [Fact]
    public void ClearMapObjectsCommand_Exists() => Assert.NotNull(_viewModel.ClearMapObjectsCommand);

    [Fact]
    public async Task ToggleMarkersCommand_AddsMarker()
    {
        // Arrange
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.GetCameraTargetAsync().Returns(Task.FromResult(new GeoCoordinates(0, 0)));
        mockMapService.AddMapMarker(Arg.Any<MapMarker>());
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.ToggleMarkersCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        mockMapService.Received(1).AddMapMarker(Arg.Any<MapMarker>());
    }

    [Fact]
    public async Task ToggleCirclesCommand_AddsCircle()
    {
        // Arrange
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.GetCameraTargetAsync().Returns(Task.FromResult(new GeoCoordinates(0, 0)));
        mockMapService.AddMapCircle(Arg.Any<MapCircle>());
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.ToggleCirclesCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        mockMapService.Received(1).AddMapCircle(Arg.Any<MapCircle>());
    }

    [Fact]
    public async Task TogglePolylinesCommand_AddsPolyline()
    {
        // Arrange
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.GetCameraTargetAsync().Returns(Task.FromResult(new GeoCoordinates(0, 0)));
        mockMapService.AddMapPolyline(Arg.Any<MapPolyline>());
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.TogglePolylinesCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        mockMapService.Received(1).AddMapPolyline(Arg.Any<MapPolyline>());
    }

    [Fact]
    public async Task TogglePolygonsCommand_AddsPolygon()
    {
        // Arrange
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.GetCameraTargetAsync().Returns(Task.FromResult(new GeoCoordinates(0, 0)));
        mockMapService.AddMapPolygon(Arg.Any<MapPolygon>());
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.TogglePolygonsCommand.Execute(null);
        await Task.Delay(50);

        // Assert
        mockMapService.Received(1).AddMapPolygon(Arg.Any<MapPolygon>());
    }

    #endregion

    #region Map Style Tests

    [Fact]
    public void ChangeMapSchemeCommand_Exists() => Assert.NotNull(_viewModel.ChangeMapSchemeCommand);

    [Fact]
    public async Task ChangeMapSchemeCommand_ChangesScheme()
    {
        // Arrange
        var mockMapService = Substitute.For<IMapService>();
        mockMapService.LoadSceneAsync(Arg.Any<MapScheme>()).Returns(Task.CompletedTask);
        var mapServiceField = typeof(ModernMainViewModel).GetField("_mapService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        mapServiceField!.SetValue(_viewModel, mockMapService);

        // Act
        _viewModel.ChangeMapSchemeCommand.Execute(MapScheme.HybridDay);
        await Task.Delay(50);

        // Assert
        await mockMapService.Received(1).LoadSceneAsync(MapScheme.HybridDay);
        Assert.Equal(MapScheme.HybridDay, _viewModel.SelectedScheme);
    }

    #endregion

    public void Dispose() => _viewModel.Dispose();
}
