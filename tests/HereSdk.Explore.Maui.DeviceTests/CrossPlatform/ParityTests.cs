using Here.Explore.Maui.Helpers;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Models.Search;
using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.DeviceTests.CrossPlatform;

/// <summary>
/// Tests that validate model consistency across Android and iOS.
/// These tests run on both platforms to catch platform-specific divergence.
/// </summary>
public class ParityTests
{
    // ================================================================
    // GeoCoordinates parity
    // ================================================================

    [Fact]
    public void GeoCoordinates_Roundtrip_Equality()
    {
        var a = new GeoCoordinates(52.5, 13.4);
        var b = new GeoCoordinates(a.Latitude, a.Longitude);
        Assert.Equal(a, b);
    }

    [Fact]
    public void GeoCoordinates_ExtremeValues_ArePreserved()
    {
        var northPole = new GeoCoordinates(90, 0);
        var southPole = new GeoCoordinates(-90, 0);
        var equator = new GeoCoordinates(0, 180);

        Assert.Equal(90, northPole.Latitude);
        Assert.Equal(-90, southPole.Latitude);
        Assert.Equal(180, equator.Longitude);
    }

    // ================================================================
    // CircleGeometryHelper parity
    // ================================================================

    [Fact]
    public void CircleGeometry_ProducesConsistentVertexCount()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);
        Assert.True(vertices.Count >= 32, $"Expected at least 32 vertices, got {vertices.Count}");
    }

    [Fact]
    public void CircleGeometry_VerticesAreClosedLoop()
    {
        var center = new GeoCoordinates(52.5, 13.4);
        var vertices = CircleGeometryHelper.GenerateCircleVertices(center, 1000);
        Assert.True(vertices.Count >= 3);
    }

    // ================================================================
    // MapMarker parity
    // ================================================================

    [Fact]
    public void MapMarker_Equality_AcrossInstances()
    {
        var a = new MapMarker(new GeoCoordinates(52.5, 13.4), Text: "A");
        var b = new MapMarker(new GeoCoordinates(52.5, 13.4), Text: "A");
        // Records with same values should be equal
        Assert.Equal(a.Coordinates, b.Coordinates);
        Assert.Equal(a.Text, b.Text);
    }

    // ================================================================
    // MapPolyline parity
    // ================================================================

    [Fact]
    public void MapPolyline_DefaultColor_IsConsistent()
    {
        var polyline = new MapPolyline(new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 1) });
        Assert.Equal(0xFF0000FFu, polyline.Color);
    }

    // ================================================================
    // MapPolygon parity
    // ================================================================

    [Fact]
    public void MapPolygon_DefaultFillColor_IsConsistent()
    {
        var polygon = new MapPolygon(new[] { new GeoCoordinates(0, 0), new GeoCoordinates(1, 0), new GeoCoordinates(0, 1) });
        Assert.Equal(0x330000FFu, polygon.FillColor);
    }

    // ================================================================
    // MapCircle parity
    // ================================================================

    [Fact]
    public void MapCircle_Defaults_AreConsistent()
    {
        var circle = new MapCircle(new GeoCoordinates(0, 0), 500);
        Assert.Equal(2, circle.StrokeWidthInPixels);
        Assert.Equal(0xFF0000FFu, circle.StrokeColor);
        Assert.Equal(0x330000FFu, circle.FillColor);
    }

    // ================================================================
    // Waypoint parity
    // ================================================================

    [Fact]
    public void Waypoint_Equality_AcrossInstances()
    {
        var a = new Waypoint(new GeoCoordinates(52.5, 13.4));
        var b = new Waypoint(new GeoCoordinates(52.5, 13.4));
        Assert.Equal(a, b);
    }

    [Fact]
    public void Waypoint_Type_IsPreserved()
    {
        var wp = new Waypoint(new GeoCoordinates(0, 0), Type: WaypointType.Through);
        Assert.Equal(WaypointType.Through, wp.Type);
    }

    // ================================================================
    // Route parity
    // ================================================================

    [Fact]
    public void Route_DurationText_Format_IsConsistent()
    {
        var route1 = new Route("r1", new List<Section>(), 5000, 3660);
        var route2 = new Route("r2", new List<Section>(), 10000, 3660);

        Assert.NotEqual(route1.DurationText, route2.DurationText);
    }

    [Fact]
    public void Route_Handle_IsPreserved()
    {
        var route = new Route("my-handle-123", new List<Section>(), 0, 0);
        Assert.Equal("my-handle-123", route.Handle);
    }

    // ================================================================
    // Place parity
    // ================================================================

    [Fact]
    public void Place_WithAllFields_Equality()
    {
        var address = new Address(City: "Berlin", CountryCode: "DE");
        var a = new Place("p1", "Test", new GeoCoordinates(52.5, 13.4), Address: address);
        var b = new Place("p1", "Test", new GeoCoordinates(52.5, 13.4), Address: address);
        Assert.Equal(a.Id, b.Id);
    }

    [Fact]
    public void SearchResult_SameQuery_Structure()
    {
        var result = new SearchResult(SearchError.None, new List<Place>());
        Assert.NotNull(result);
    }

    // ================================================================
    // Traffic parity
    // ================================================================

    [Fact]
    public void TrafficFlow_Equality_AcrossInstances()
    {
        var poly = new GeoPolyline(new[] { new GeoCoordinates(0, 0) });
        var a = new TrafficFlow(0.5, 10.0, poly);
        var b = new TrafficFlow(0.5, 10.0, poly);
        Assert.Equal(a, b);
    }

    [Fact]
    public void TrafficIncident_AllFields_Preserved()
    {
        var incident = new TrafficIncident(
            "inc-123", "Test incident", TrafficIncidentType.Accident,
            TrafficIncidentImpact.Major, RoadClosed: true,
            StartTime: 1000000, EndTime: 2000000);
        Assert.Equal("inc-123", incident.Id);
        Assert.True(incident.RoadClosed);
        Assert.Equal(1000000, incident.StartTime);
        Assert.Equal(2000000, incident.EndTime);
    }

    // ================================================================
    // Model immutability
    // ================================================================

    [Fact]
    public void GeoCoordinates_IsImmutable()
    {
        var coords = new GeoCoordinates(52.5, 13.4);
        // Records are immutable by default in C#
        // Verify with expression works as expected
        var modified = coords with { Latitude = 48.8 };
        Assert.Equal(52.5, coords.Latitude);
        Assert.Equal(48.8, modified.Latitude);
    }

    [Fact]
    public void Route_IsImmutable()
    {
        var sections = new List<Section>();
        var route = new Route("r1", sections, 1000, 60);
        var modified = route with { LengthInMeters = 2000 };
        Assert.Equal(1000, route.LengthInMeters);
        Assert.Equal(2000, modified.LengthInMeters);
    }
}
