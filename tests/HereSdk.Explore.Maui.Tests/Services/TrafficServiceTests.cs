using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using NSubstitute;

namespace Here.Explore.Maui.Tests.Services;

public class TrafficServiceTests
{
    [Fact]
    public async Task QueryFlowAsync_ReturnsFlowData()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var area = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        var expected = new TrafficFlowResult(TrafficQueryError.None, new List<TrafficFlow>
        {
            new(0.5, 15.0, new GeoPolyline(new List<GeoCoordinates>()))
        });
        trafficService.QueryFlowAsync(area, Arg.Any<TrafficFlowQueryOptions>()).Returns(Task.FromResult(expected));

        var result = await trafficService.QueryFlowAsync(area, new TrafficFlowQueryOptions());

        Assert.Equal(TrafficQueryError.None, result.Error);
        Assert.NotNull(result.Flows);
        Assert.Single(result.Flows);
        Assert.Equal(0.5, result.Flows[0].JamFactor);
    }

    [Fact]
    public async Task QueryIncidentsAsync_ReturnsIncidents()
    {
        var trafficService = Substitute.For<ITrafficService>();
        var area = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        var expected = new TrafficIncidentsResult(TrafficQueryError.None, new List<TrafficIncident>
        {
            new("inc-1", "Road closed", TrafficIncidentType.RoadClosure, TrafficIncidentImpact.Major)
        });
        trafficService.QueryIncidentsAsync(area, Arg.Any<TrafficIncidentsQueryOptions>()).Returns(Task.FromResult(expected));

        var result = await trafficService.QueryIncidentsAsync(area, new TrafficIncidentsQueryOptions());

        Assert.Equal(TrafficQueryError.None, result.Error);
        Assert.NotNull(result.Incidents);
        Assert.Single(result.Incidents);
        Assert.Equal("inc-1", result.Incidents[0].Id);
    }
}