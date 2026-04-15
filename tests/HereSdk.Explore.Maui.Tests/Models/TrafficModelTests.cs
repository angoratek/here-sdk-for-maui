using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.Tests.Models;

public class TrafficModelTests
{
    [Fact]
    public void TrafficFlow_CreatedWithJamFactor()
    {
        var geometry = new GeoPolyline(new List<GeoCoordinates>
        {
            new(52.5, 13.4),
            new(52.6, 13.5)
        });
        var flow = new TrafficFlow(7.5, 30.0, geometry);

        Assert.Equal(7.5, flow.JamFactor);
        Assert.Equal(30.0, flow.SpeedInMetersPerSecond);
        Assert.Equal(2, flow.Geometry.Vertices.Count);
    }

    [Fact]
    public void TrafficIncident_CreatedWithRequiredFields()
    {
        var incident = new TrafficIncident("inc-1", "Road closed due to construction",
            TrafficIncidentType.Construction, TrafficIncidentImpact.Major);

        Assert.Equal("inc-1", incident.Id);
        Assert.Equal("Road closed due to construction", incident.Description);
        Assert.Equal(TrafficIncidentType.Construction, incident.Type);
        Assert.Equal(TrafficIncidentImpact.Major, incident.Impact);
        Assert.Null(incident.RoadClosed);
    }

    [Fact]
    public void TrafficIncident_CreatedWithRoadClosed()
    {
        var incident = new TrafficIncident("inc-2", "Full road closure",
            TrafficIncidentType.RoadClosure, TrafficIncidentImpact.Closed, RoadClosed: true);

        Assert.True(incident.RoadClosed!.Value);
        Assert.Equal(TrafficIncidentImpact.Closed, incident.Impact);
    }

    [Fact]
    public void TrafficFlowResult_WithFlows()
    {
        var flows = new List<TrafficFlow>
        {
            new(0.0, 50.0, new GeoPolyline(new List<GeoCoordinates>())),
            new(4.0, 25.0, new GeoPolyline(new List<GeoCoordinates>()))
        };
        var result = new TrafficFlowResult(TrafficQueryError.None, flows);

        Assert.Equal(TrafficQueryError.None, result.Error);
        Assert.Equal(2, result.Flows!.Count);
    }

    [Fact]
    public void TrafficFlowResult_WithError()
    {
        var result = new TrafficFlowResult(TrafficQueryError.NetworkError, null);

        Assert.Equal(TrafficQueryError.NetworkError, result.Error);
        Assert.Null(result.Flows);
    }

    [Fact]
    public void TrafficIncidentsResult_WithIncidents()
    {
        var incidents = new List<TrafficIncident>
        {
            new("i1", "Accident", TrafficIncidentType.Accident, TrafficIncidentImpact.Moderate)
        };
        var result = new TrafficIncidentsResult(TrafficQueryError.None, incidents);

        Assert.Equal(TrafficQueryError.None, result.Error);
        Assert.Single(result.Incidents!);
    }

    [Fact]
    public void TrafficQueryError_Values()
    {
        Assert.Equal(0, (int)TrafficQueryError.None);
        Assert.Equal(1, (int)TrafficQueryError.NetworkError);
    }

    [Fact]
    public void TrafficIncidentType_DistinctValues()
    {
        var types = new[]
        {
            TrafficIncidentType.Unknown,
            TrafficIncidentType.Accident,
            TrafficIncidentType.Congestion,
            TrafficIncidentType.DisabledVehicle,
            TrafficIncidentType.LaneRestriction,
            TrafficIncidentType.RoadClosure,
            TrafficIncidentType.RoadHazard,
            TrafficIncidentType.Construction,
            TrafficIncidentType.MassTransit,
            TrafficIncidentType.PlannedEvent,
            TrafficIncidentType.Weather,
            TrafficIncidentType.Miscellaneous
        };
        Assert.Equal(12, types.Distinct().Count());
    }

    [Fact]
    public void TrafficIncidentImpact_DistinctValues()
    {
        var impacts = new[]
        {
            TrafficIncidentImpact.Unknown,
            TrafficIncidentImpact.Minor,
            TrafficIncidentImpact.Moderate,
            TrafficIncidentImpact.Major,
            TrafficIncidentImpact.Closed
        };
        Assert.Equal(5, impacts.Distinct().Count());
    }

    [Fact]
    public void TrafficFlowQueryOptions_Defaults()
    {
        var opts = new TrafficFlowQueryOptions();

        Assert.NotNull(opts);
    }

    [Fact]
    public void TrafficIncidentsQueryOptions_Defaults()
    {
        var opts = new TrafficIncidentsQueryOptions();

        Assert.NotNull(opts);
    }

    [Fact]
    public void TrafficIncidentLookupOptions_Defaults()
    {
        var opts = new TrafficIncidentLookupOptions();

        Assert.NotNull(opts);
    }
}