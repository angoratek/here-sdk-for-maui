using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.DeviceTests.Android;

public class TrafficServiceAndroidTests
{
    // ================================================================
    // Service lifecycle
    // ================================================================

    [Fact]
    public void TrafficService_CanBeInstantiated()
    {
        var service = new TrafficService();
        Assert.NotNull(service);
    }

    // ================================================================
    // TrafficFlow model
    // ================================================================

    [Fact]
    public void TrafficFlow_CanBeCreated()
    {
        var polyline = new GeoPolyline(new[] { new GeoCoordinates(52.5, 13.4), new GeoCoordinates(52.6, 13.5) });
        var flow = new TrafficFlow(0.5, 15.0, polyline);
        Assert.Equal(0.5, flow.JamFactor);
        Assert.Equal(15.0, flow.SpeedInMetersPerSecond);
        Assert.Equal(2, flow.Geometry.Vertices.Count);
    }

    [Fact]
    public void TrafficFlow_WithUncertainty()
    {
        var polyline = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        var flow = new TrafficFlow(0.8, 5.0, polyline,
            FreeFlowSpeedInMetersPerSecond: 20.0,
            JamFactorUncertainty: 0.1);
        Assert.Equal(20.0, flow.FreeFlowSpeedInMetersPerSecond);
        Assert.Equal(0.1, flow.JamFactorUncertainty);
    }

    [Fact]
    public void TrafficFlow_Equality_Works()
    {
        var polyline = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        var a = new TrafficFlow(0.5, 10.0, polyline);
        var b = new TrafficFlow(0.5, 10.0, polyline);
        Assert.Equal(a, b);
    }

    // ================================================================
    // TrafficIncident model
    // ================================================================

    [Fact]
    public void TrafficIncident_CanBeCreated()
    {
        var incident = new TrafficIncident(
            "inc1",
            "Accident on A100",
            TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major);
        Assert.Equal("inc1", incident.Id);
        Assert.Equal(TrafficIncidentType.Accident, incident.Type);
        Assert.Equal(TrafficIncidentImpact.Major, incident.Impact);
    }

    [Fact]
    public void TrafficIncident_WithGeometry()
    {
        var polyline = new GeoPolyline(new[] { new GeoCoordinates(52.5, 13.4) });
        var incident = new TrafficIncident(
            "inc2", "Construction", TrafficIncidentType.Construction,
            TrafficIncidentImpact.Moderate, Geometry: polyline);
        Assert.NotNull(incident.Geometry);
    }

    [Fact]
    public void TrafficIncident_WithTimeWindow()
    {
        var incident = new TrafficIncident(
            "inc3", "Planned event", TrafficIncidentType.PlannedEvent,
            TrafficIncidentImpact.Minor,
            StartTime: 1715700000, EndTime: 1715786400);
        Assert.Equal(1715700000, incident.StartTime);
        Assert.Equal(1715786400, incident.EndTime);
    }

    [Fact]
    public void TrafficIncident_RoadClosed()
    {
        var incident = new TrafficIncident(
            "inc4", "Street flooded", TrafficIncidentType.Weather,
            TrafficIncidentImpact.Closed, RoadClosed: true);
        Assert.True(incident.RoadClosed);
    }

    // ================================================================
    // GeoPolyline model
    // ================================================================

    [Fact]
    public void GeoPolyline_CanBeCreated()
    {
        var polyline = new GeoPolyline(new[] {
            new GeoCoordinates(52.5, 13.4),
            new GeoCoordinates(52.6, 13.5),
            new GeoCoordinates(52.7, 13.6) });
        Assert.Equal(3, polyline.Vertices.Count);
    }

    [Fact]
    public void GeoPolyline_Equality_Works()
    {
        var a = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        var b = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        Assert.Equal(a, b);
    }

    // ================================================================
    // GeoCircle model
    // ================================================================

    [Fact]
    public void GeoCircle_CanBeCreated()
    {
        var circle = new GeoCircle(new GeoCoordinates(52.5, 13.4), 5000);
        Assert.Equal(5000, circle.RadiusInMeters);
        Assert.Equal(52.5, circle.Center.Latitude);
    }

    // ================================================================
    // Result models
    // ================================================================

    [Fact]
    public void TrafficFlowResult_Success()
    {
        var polyline = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        var flows = new[] { new TrafficFlow(0.3, 20.0, polyline) };
        var result = new TrafficFlowResult(TrafficQueryError.None, flows);
        Assert.Single(result.Flows!);
    }

    [Fact]
    public void TrafficFlowResult_Error()
    {
        var result = new TrafficFlowResult(TrafficQueryError.NetworkError, null);
        Assert.Equal(TrafficQueryError.NetworkError, result.Error);
        Assert.Null(result.Flows);
    }

    [Fact]
    public void TrafficIncidentsResult_Success()
    {
        var incidents = new[] {
            new TrafficIncident("i1", "Accident", TrafficIncidentType.Accident, TrafficIncidentImpact.Major) };
        var result = new TrafficIncidentsResult(TrafficQueryError.None, incidents);
        Assert.Single(result.Incidents!);
    }

    [Fact]
    public void TrafficIncidentsResult_Error()
    {
        var result = new TrafficIncidentsResult(TrafficQueryError.NoResults, null);
        Assert.Equal(TrafficQueryError.NoResults, result.Error);
    }

    // ================================================================
    // Query options models
    // ================================================================

    [Fact]
    public void TrafficFlowQueryOptions_CanBeCreated()
    {
        var options = new TrafficFlowQueryOptions();
        Assert.NotNull(options);
    }

    [Fact]
    public void TrafficIncidentsQueryOptions_CanBeCreated()
    {
        var options = new TrafficIncidentsQueryOptions();
        Assert.NotNull(options);
    }

    [Fact]
    public void TrafficIncidentLookupOptions_CanBeCreated()
    {
        var options = new TrafficIncidentLookupOptions();
        Assert.NotNull(options);
    }

    // ================================================================
    // TrafficIncidentType enum
    // ================================================================

    [Fact]
    public void TrafficIncidentType_AllValues_AreDefined()
    {
        var types = Enum.GetValues<TrafficIncidentType>();
        Assert.Contains(TrafficIncidentType.Accident, types);
        Assert.Contains(TrafficIncidentType.Congestion, types);
        Assert.Contains(TrafficIncidentType.Construction, types);
        Assert.Contains(TrafficIncidentType.RoadClosure, types);
        Assert.Contains(TrafficIncidentType.RoadHazard, types);
        Assert.Contains(TrafficIncidentType.MassTransit, types);
        Assert.Contains(TrafficIncidentType.PlannedEvent, types);
        Assert.Contains(TrafficIncidentType.Weather, types);
        Assert.Contains(TrafficIncidentType.Unknown, types);
    }

    // ================================================================
    // TrafficIncidentImpact enum
    // ================================================================

    [Fact]
    public void TrafficIncidentImpact_AllValues_AreDefined()
    {
        var impacts = Enum.GetValues<TrafficIncidentImpact>();
        Assert.Contains(TrafficIncidentImpact.Minor, impacts);
        Assert.Contains(TrafficIncidentImpact.Moderate, impacts);
        Assert.Contains(TrafficIncidentImpact.Major, impacts);
        Assert.Contains(TrafficIncidentImpact.Closed, impacts);
        Assert.Contains(TrafficIncidentImpact.Unknown, impacts);
    }

    // ================================================================
    // TrafficQueryError enum
    // ================================================================

    [Fact]
    public void TrafficQueryError_Values_AreDefined()
    {
        var errors = Enum.GetValues<TrafficQueryError>();
        Assert.Contains(TrafficQueryError.None, errors);
        Assert.Contains(TrafficQueryError.NetworkError, errors);
        Assert.Contains(TrafficQueryError.NoResults, errors);
        Assert.Contains(TrafficQueryError.EngineNotInitialized, errors);
    }
}
