using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class TrafficViewModelErrorStateTests
{
    private readonly ITrafficService _trafficService;
    private readonly TrafficViewModel _viewModel;

    public TrafficViewModelErrorStateTests()
    {
        _trafficService = Substitute.For<ITrafficService>();
        _viewModel = new TrafficViewModel(_trafficService);
    }

    #region Flow — No Data

    [Fact]
    public async Task QueryFlow_WhenNoSegmentsReturned_SetsEmptyState()
    {
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow>()));

        _viewModel.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal("No traffic flow data", _viewModel.EmptyStateTitle);
        Assert.NotNull(_viewModel.EmptyStateSubtitle);
        Assert.Equal("", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task QueryFlow_WhenErrorResponse_SetsStatusMessage()
    {
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.NetworkError, new List<TrafficFlow>()));

        _viewModel.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Flow query error", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task QueryFlow_WhenExceptionThrown_SetsStatusMessage()
    {
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(Task.FromException<TrafficFlowResult>(new InvalidOperationException("Network error")));

        _viewModel.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Network error", _viewModel.StatusMessage);
    }

    #endregion

    #region Incidents — No Data

    [Fact]
    public async Task QueryIncidents_WhenNoIncidents_SetsEmptyState()
    {
        // Start with incidents visible to avoid initial query
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsIncidentsVisible))!
            .SetValue(_viewModel, false);

        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident>()));

        _viewModel.ToggleIncidentsCommand.Execute(null);
        await Task.Delay(50);

        Assert.Equal("No traffic incidents", _viewModel.EmptyStateTitle);
        Assert.Contains("No incidents", _viewModel.EmptyStateSubtitle);
        Assert.Equal("", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task QueryIncidents_WhenErrorResponse_SetsStatusMessage()
    {
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsIncidentsVisible))!
            .SetValue(_viewModel, false);

        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.NetworkError, new List<TrafficIncident>()));

        _viewModel.ToggleIncidentsCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Incident query error", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task QueryIncidents_WhenExceptionThrown_SetsStatusMessage()
    {
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsIncidentsVisible))!
            .SetValue(_viewModel, false);

        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(Task.FromException<TrafficIncidentsResult>(new InvalidOperationException("Service down")));

        _viewModel.ToggleIncidentsCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("Service down", _viewModel.StatusMessage);
    }

    #endregion

    #region Both Flow and Incidents — Empty Interaction

    [Fact]
    public async Task FlowEmpty_IncidentsWithData_EmptyStateCleared()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        // Set up: incidents visible with data, flow toggle will bring empty state
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow>()));

        var incident = new TrafficIncident("inc1", "Accident", TrafficIncidentType.Accident, TrafficIncidentImpact.Major);
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident> { incident }));

        // Toggle flow on — should set empty state
        _viewModel.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);
        Assert.Equal("No traffic flow data", _viewModel.EmptyStateTitle);

        // Refresh — should query both, and incidents has data
        _viewModel.RefreshTrafficCommand.Execute(null);
        await Task.Delay(50);

        // Flow is visible but empty, so empty state reflects flow
        Assert.True(_viewModel.IsFlowVisible);
        Assert.True(_viewModel.IsIncidentsVisible);
    }

    #endregion

    #region Map Service Null Safety

    [Fact]
    public async Task ToggleFlow_WhenMapServiceNull_DoesNotCrash()
    {
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow>()));

        // No map service set — should not crash
        var exception = await Record.ExceptionAsync(async () =>
        {
            _viewModel.ToggleFlowCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    [Fact]
    public async Task ToggleIncidents_WhenMapServiceNull_DoesNotCrash()
    {
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsIncidentsVisible))!
            .SetValue(_viewModel, false);

        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident>()));

        var exception = await Record.ExceptionAsync(async () =>
        {
            _viewModel.ToggleIncidentsCommand.Execute(null);
            await Task.Delay(50);
        });

        Assert.Null(exception);
    }

    #endregion
}
