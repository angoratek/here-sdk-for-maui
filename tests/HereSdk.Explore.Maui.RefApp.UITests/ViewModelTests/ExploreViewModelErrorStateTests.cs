using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class ExploreViewModelErrorStateTests
{
    private readonly ISearchService _searchService;
    private readonly ILocationService _locationService;
    private readonly IConnectivityService _connectivityService;
    private readonly IPermissionsService _permissionsService;
    private readonly ExploreViewModel _viewModel;

    public ExploreViewModelErrorStateTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _locationService = Substitute.For<ILocationService>();
        _connectivityService = Substitute.For<IConnectivityService>();
        _permissionsService = Substitute.For<IPermissionsService>();
        _connectivityService.IsConnected.Returns(true);
        _permissionsService.RequestLocationPermissionAsync().Returns(true);
        _viewModel = new ExploreViewModel(_searchService, _locationService, _connectivityService, _permissionsService);
    }

    #region Search Error States

    [Fact]
    public async Task SubmitSearch_WhenServiceReturnsError_SetsSearchErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _viewModel.SearchQuery = "SF";
        _searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.NetworkError, new List<Place>()));

        _viewModel.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("NetworkError", _viewModel.SearchErrorMessage);
    }

    [Fact]
    public async Task SubmitSearch_WhenNoResults_SetsEmptyState()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _viewModel.SearchQuery = "qzxyunknown";
        _searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>()));

        _viewModel.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal("No places found", _viewModel.EmptyStateTitle);
        Assert.NotNull(_viewModel.EmptyStateSubtitle);
    }

    [Fact]
    public async Task SubmitSearch_WhenExceptionThrown_SetsSearchErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _viewModel.SearchQuery = "SF";
        _searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromException<SearchResult>(new InvalidOperationException("Network down")));

        _viewModel.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("Network down", _viewModel.SearchErrorMessage);
    }

    [Fact]
    public async Task SubmitSearch_WhenSuccessful_ClearsPreviousErrors()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        // Set previous error state
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.SearchErrorMessage))!
            .SetValue(_viewModel, "Previous error");

        _viewModel.SearchQuery = "SF";
        var place = new Place("id1", "San Francisco", new GeoCoordinates(37.77, -122.42));
        _searchService.SearchAsync(Arg.Any<TextQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place> { place }));

        _viewModel.SubmitSearchCommand.Execute(null);
        await Task.Delay(50);

        Assert.Null(_viewModel.SearchErrorMessage);
        Assert.Null(_viewModel.EmptyStateTitle);
    }

    #endregion

    #region Category Search Error States

    [Fact]
    public async Task SearchCategory_WhenServiceReturnsError_SetsSearchErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.NetworkError, new List<Place>()));

        _viewModel.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("Category search failed", _viewModel.SearchErrorMessage);
    }

    [Fact]
    public async Task SearchCategory_WhenNoResults_SetsEmptyState()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(new SearchResult(SearchError.None, new List<Place>()));

        _viewModel.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        Assert.Equal("No places found", _viewModel.EmptyStateTitle);
        Assert.Contains("restaurant", _viewModel.EmptyStateSubtitle);
    }

    [Fact]
    public async Task SearchCategory_WhenExceptionThrown_SetsSearchErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(37.77, -122.42));
        _viewModel.SetMapService(mockMap);

        _searchService.SearchAsync(Arg.Any<CategoryQuery>(), Arg.Any<SearchOptions>())
            .Returns(Task.FromException<SearchResult>(new InvalidOperationException("Network down")));

        _viewModel.SearchCategoryCommand.Execute("restaurant");
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("Network down", _viewModel.SearchErrorMessage);
    }

    #endregion

    #region Location Permission States

    [Fact]
    public async Task CenterOnLocation_WhenPermissionDenied_SetsErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);
        _permissionsService.RequestLocationPermissionAsync().Returns(false);

        _viewModel.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("permission denied", _viewModel.SearchErrorMessage);
        Assert.False(_viewModel.IsLocationTracking);
    }

    [Fact]
    public async Task CenterOnLocation_WhenPermissionGrantedButNoLocation_SetsError()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);
        _permissionsService.RequestLocationPermissionAsync().Returns(true);
        _locationService.GetCurrentLocationAsync().Returns((Models.Location?)null);

        _viewModel.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("Could not determine", _viewModel.SearchErrorMessage);
    }

    [Fact]
    public async Task CenterOnLocation_WhenLocationServiceThrows_SetsErrorMessage()
    {
        var mockMap = Substitute.For<IMapService>();
        _viewModel.SetMapService(mockMap);
        _permissionsService.RequestLocationPermissionAsync().Returns(true);
        _locationService.GetCurrentLocationAsync()
            .Returns(Task.FromException<Models.Location?>(new InvalidOperationException("GPS hardware error")));

        _viewModel.CenterOnLocationCommand.Execute(null);
        await Task.Delay(50);

        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("GPS hardware error", _viewModel.SearchErrorMessage);
    }

    #endregion

    #region Offline / Connectivity States

    [Fact]
    public void ConnectivityChanged_WhenOffline_SetsSearchErrorMessage()
    {
        typeof(ExploreViewModel).GetMethod("OnConnectivityChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_viewModel, new object[] { null!, false });

        Assert.True(_viewModel.IsOffline);
        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("offline", _viewModel.SearchErrorMessage);
    }

    [Fact]
    public void ConnectivityChanged_WhenBackOnline_ClearsOfflineErrorMessage()
    {
        var method = typeof(ExploreViewModel).GetMethod("OnConnectivityChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // First go offline
        method.Invoke(_viewModel, new object[] { null!, false });
        Assert.NotNull(_viewModel.SearchErrorMessage);

        // Then back online
        method.Invoke(_viewModel, new object[] { null!, true });

        Assert.False(_viewModel.IsOffline);
        Assert.Null(_viewModel.SearchErrorMessage);
    }

    [Fact]
    public void ConnectivityChanged_WhenOnline_DoesNotClearNonOfflineError()
    {
        // Set a non-offline error
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.SearchErrorMessage))!
            .SetValue(_viewModel, "Search failed: NetworkError");

        typeof(ExploreViewModel).GetMethod("OnConnectivityChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_viewModel, new object[] { null!, true });

        // Non-offline error should remain
        Assert.NotNull(_viewModel.SearchErrorMessage);
        Assert.Contains("NetworkError", _viewModel.SearchErrorMessage);
    }

    #endregion

    #region ClearSearch

    [Fact]
    public void ClearSearch_ClearsErrorAndEmptyStates()
    {
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.SearchErrorMessage))!
            .SetValue(_viewModel, "Some error");
        typeof(ExploreViewModel).GetProperty(nameof(ExploreViewModel.EmptyStateTitle))!
            .SetValue(_viewModel, "No results");

        _viewModel.ClearSearchCommand.Execute(null);

        Assert.Null(_viewModel.SearchErrorMessage);
        Assert.Null(_viewModel.EmptyStateTitle);
    }

    #endregion
}
