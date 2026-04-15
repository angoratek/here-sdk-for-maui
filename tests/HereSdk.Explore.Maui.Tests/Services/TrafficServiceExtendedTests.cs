using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class TrafficServiceExtendedTests
{
    [Fact]
    public async Task QueryFlowAsync_WithNetworkError()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var area = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        trafficService.QueryFlowAsync(area, Arg.Any<TrafficFlowQueryOptions>())
            .Returns(Task.FromResult(new TrafficFlowResult(TrafficQueryError.NetworkError, null)));

        var result = await trafficService.QueryFlowAsync(area, new TrafficFlowQueryOptions());

        Assert.Equal(TrafficQueryError.NetworkError, result.Error);
        Assert.Null(result.Flows);
    }

    [Fact]
    public async Task QueryIncidentsAsync_MultipleIncidents()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var area = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        var incidents = new List<TrafficIncident>
        {
            new("inc-1", "Accident on A9", TrafficIncidentType.Accident, TrafficIncidentImpact.Major),
            new("inc-2", "Construction on A100", TrafficIncidentType.Construction, TrafficIncidentImpact.Minor),
            new("inc-3", "Congestion near Potsdam", TrafficIncidentType.Congestion, TrafficIncidentImpact.Moderate)
        };
        var expected = new TrafficIncidentsResult(TrafficQueryError.None, incidents);
        trafficService.QueryIncidentsAsync(area, Arg.Any<TrafficIncidentsQueryOptions>())
            .Returns(Task.FromResult(expected));

        var result = await trafficService.QueryIncidentsAsync(area, new TrafficIncidentsQueryOptions());

        Assert.Equal(TrafficQueryError.None, result.Error);
        Assert.Equal(3, result.Incidents!.Count);
    }

    [Fact]
    public async Task LookupIncidentAsync_ReturnsIncident()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var incident = new TrafficIncident("inc-1", "Road works", TrafficIncidentType.Construction,
            TrafficIncidentImpact.Minor, RoadClosed: false);
        trafficService.LookupIncidentAsync("inc-1", Arg.Any<TrafficIncidentLookupOptions>())
            .Returns(Task.FromResult<TrafficIncident?>(incident));

        var result = await trafficService.LookupIncidentAsync("inc-1", new TrafficIncidentLookupOptions());

        Assert.NotNull(result);
        Assert.Equal("inc-1", result!.Id);
        Assert.Equal(TrafficIncidentType.Construction, result.Type);
    }

    [Fact]
    public async Task LookupIncidentAsync_ReturnsNull_WhenNotFound()
    {
        var trafficService = Substitute.For<ITrafficService>();
        trafficService.LookupIncidentAsync("nonexistent", Arg.Any<TrafficIncidentLookupOptions>())
            .Returns(Task.FromResult<TrafficIncident?>(null));

        var result = await trafficService.LookupIncidentAsync("nonexistent", new TrafficIncidentLookupOptions());

        Assert.Null(result);
    }

    [Fact]
    public async Task QueryFlowAsync_MultipleFlowSegments()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var area = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        var flows = new List<TrafficFlow>
        {
            new(0.0, 30.0, new GeoPolyline(new List<GeoCoordinates>()), FreeFlowSpeedInMetersPerSecond: 30.0),
            new(4.0, 15.0, new GeoPolyline(new List<GeoCoordinates>()), FreeFlowSpeedInMetersPerSecond: 30.0),
            new(10.0, 5.0, new GeoPolyline(new List<GeoCoordinates>()), FreeFlowSpeedInMetersPerSecond: 30.0)
        };
        var expected = new TrafficFlowResult(TrafficQueryError.None, flows);
        trafficService.QueryFlowAsync(area, Arg.Any<TrafficFlowQueryOptions>())
            .Returns(Task.FromResult(expected));

        var result = await trafficService.QueryFlowAsync(area, new TrafficFlowQueryOptions());

        Assert.Equal(3, result.Flows!.Count);
        Assert.Equal(10.0, result.Flows[2].JamFactor);
    }

    [Fact]
    public void TrafficIncident_WithAllOptionalFields()
    {
        var incident = new TrafficIncident(
            "inc-1", "Major accident", TrafficIncidentType.Accident, TrafficIncidentImpact.Closed,
            Geometry: new GeoPolyline(new List<GeoCoordinates> { new(52.5, 13.4) }),
            StartTime: 1000000, EndTime: 2000000, RoadClosed: true);

        Assert.True(incident.RoadClosed);
        Assert.NotNull(incident.Geometry);
        Assert.Equal(1000000, incident.StartTime);
        Assert.Equal(2000000, incident.EndTime);
    }

    [Fact]
    public void TrafficFlow_WithJamFactorUncertainty()
    {
        var flow = new TrafficFlow(
            5.0, 20.0, new GeoPolyline(new List<GeoCoordinates>()),
            FreeFlowSpeedInMetersPerSecond: 30.0, JamFactorUncertainty: 0.5);

        Assert.Equal(5.0, flow.JamFactor);
        Assert.Equal(0.5, flow.JamFactorUncertainty);
    }
}