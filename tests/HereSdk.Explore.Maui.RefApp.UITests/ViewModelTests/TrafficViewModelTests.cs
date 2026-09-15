using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.ViewModels;
using NSubstitute;

namespace Here.Explore.Maui.RefApp.UITests.ViewModelTests;

public class TrafficViewModelTests
{
    private readonly ITrafficService _trafficService;
    private readonly TrafficViewModel _viewModel;

    public TrafficViewModelTests()
    {
        _trafficService = Substitute.For<ITrafficService>();
        _viewModel = new TrafficViewModel(_trafficService);
    }

    #region Initial State

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        Assert.False(_viewModel.IsFlowVisible);
        Assert.True(_viewModel.IsIncidentsVisible);
        Assert.False(_viewModel.IsLoading);
        Assert.Equal(0, _viewModel.IncidentCount);
        Assert.Equal(0, _viewModel.FlowCount);
    }

    #endregion

    #region ToggleFlow

    [Fact]
    public async Task ToggleFlow_WhenEnabled_QueriesAndRendersFlow()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        _viewModel.SetMapService(mockMap);

        var geometry = new GeoPolyline(new List<GeoCoordinates>
        {
            new(52.53, 13.39),
            new(52.54, 13.40)
        });
        var flow = new TrafficFlow(3.0, 15.0, geometry);
        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow> { flow }));

        _viewModel.ToggleFlowCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(_viewModel.IsFlowVisible);
        await _trafficService.Received(1).QueryFlowAsync(
            Arg.Any<GeoCircle>(),
            Arg.Any<TrafficFlowQueryOptions>());
        mockMap.Received(1).AddMapPolyline(Arg.Any<MapPolyline>());
        Assert.Equal(1, _viewModel.FlowCount);
    }

    [Fact]
    public void ToggleFlow_WhenDisabled_HidesFlow()
    {
        _viewModel.ToggleFlowCommand.Execute(null); // enable
        _viewModel.ToggleFlowCommand.Execute(null); // disable

        Assert.False(_viewModel.IsFlowVisible);
    }

    #endregion

    #region ToggleIncidents

    [Fact]
    public async Task ToggleIncidents_WhenEnabled_QueriesIncidents()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        // Start with incidents disabled so toggle enables them
        typeof(TrafficViewModel).GetProperty(nameof(TrafficViewModel.IsIncidentsVisible))!
            .SetValue(_viewModel, false);

        var incident = new TrafficIncident("inc1", "Accident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major);
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident> { incident }));

        _viewModel.ToggleIncidentsCommand.Execute(null);
        await Task.Delay(50);

        Assert.True(_viewModel.IsIncidentsVisible);
        await _trafficService.Received(1).QueryIncidentsAsync(
            Arg.Any<GeoCircle>(),
            Arg.Any<TrafficIncidentsQueryOptions>());
        Assert.Equal(1, _viewModel.IncidentCount);
    }

    [Fact]
    public void ToggleIncidents_WhenDisabled_HidesIncidents()
    {
        // Default is true, so one toggle sets it to false
        _viewModel.ToggleIncidentsCommand.Execute(null);

        Assert.False(_viewModel.IsIncidentsVisible);
    }

    #endregion

    #region RefreshTraffic

    [Fact]
    public async Task RefreshTraffic_QueriesBothIfVisible()
    {
        var mockMap = Substitute.For<IMapService>();
        mockMap.GetCameraTargetAsync().Returns(new GeoCoordinates(52.53, 13.39));
        mockMap.AddMapPolyline(Arg.Any<MapPolyline>());
        mockMap.AddMapMarker(Arg.Any<MapMarker>());
        _viewModel.SetMapService(mockMap);

        _trafficService.QueryFlowAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficFlowQueryOptions>())
            .Returns(new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow>()));
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident>()));

        _viewModel.ToggleFlowCommand.Execute(null);  // 1st flow query
        await Task.Delay(50);

        _viewModel.RefreshTrafficCommand.Execute(null);  // 2nd flow query + 1st incidents query
        await Task.Delay(50);

        // ToggleFlow queries flow once, RefreshTraffic queries both
        await _trafficService.Received(2).QueryFlowAsync(
            Arg.Any<GeoCircle>(),
            Arg.Any<TrafficFlowQueryOptions>());
        await _trafficService.Received(1).QueryIncidentsAsync(
            Arg.Any<GeoCircle>(),
            Arg.Any<TrafficIncidentsQueryOptions>());
    }

    #endregion

    #region SelectIncident

    [Fact]
    public async Task SelectIncident_LooksUpDetails()
    {
        var incident = new TrafficIncident("inc1", "Accident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major);
        _trafficService.LookupIncidentAsync("inc1", Arg.Any<TrafficIncidentLookupOptions>())
            .Returns(incident);

        _viewModel.SelectIncidentCommand.Execute(incident);
        await Task.Delay(50);

        await _trafficService.Received(1).LookupIncidentAsync("inc1",
            Arg.Any<TrafficIncidentLookupOptions>());
        Assert.NotNull(_viewModel.SelectedIncident);
    }

    #endregion

    #region StatusSeverity

    [Fact]
    public async Task QueryIncidents_Success_ShowsInfoSeverity()
    {
        var incident = new TrafficIncident("i1", "Closed road", TrafficIncidentType.RoadClosure,
            TrafficIncidentImpact.Closed);
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident> { incident }));

        _viewModel.ToggleIncidentsCommand.Execute(null); // visible=true → hide first
        _viewModel.ToggleIncidentsCommand.Execute(null); // visible=false → query
        await Task.Delay(50);

        Assert.Equal(BannerSeverity.Info, _viewModel.StatusSeverity);
        Assert.Contains("incidents found", _viewModel.StatusMessage);
    }

    [Fact]
    public async Task QueryIncidents_Error_ShowsErrorSeverity()
    {
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.NetworkError, null));

        _viewModel.ToggleIncidentsCommand.Execute(null); // hide
        _viewModel.ToggleIncidentsCommand.Execute(null); // show → query
        await Task.Delay(50);

        Assert.Equal(BannerSeverity.Error, _viewModel.StatusSeverity);
    }

    [Fact]
    public async Task QueryIncidents_ErrorThenSuccess_RestoresInfoSeverity()
    {
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.NetworkError, null));
        _viewModel.ToggleIncidentsCommand.Execute(null); // hide
        _viewModel.ToggleIncidentsCommand.Execute(null); // show → query
        await Task.Delay(50);
        Assert.Equal(BannerSeverity.Error, _viewModel.StatusSeverity);

        var incident = new TrafficIncident("i1", "Closed road", TrafficIncidentType.RoadClosure,
            TrafficIncidentImpact.Closed);
        _trafficService.QueryIncidentsAsync(Arg.Any<GeoCircle>(), Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident> { incident }));
        await Task.Delay(50);
        _viewModel.ToggleIncidentsCommand.Execute(null); // hide
        _viewModel.ToggleIncidentsCommand.Execute(null); // show again → query
        await Task.Delay(50);

        Assert.Equal(BannerSeverity.Info, _viewModel.StatusSeverity);
    }

    #endregion
}

