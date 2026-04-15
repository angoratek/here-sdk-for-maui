using Xunit;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.Tests.Models;

public class RouteDetailModelTests
{
    // --- RouteHandle ---

    [Fact]
    public void RouteHandle_Created()
    {
        var handle = new RouteHandle("route-abc-123");

        Assert.Equal("route-abc-123", handle.Id);
    }

    [Fact]
    public void RouteHandle_Equality()
    {
        var h1 = new RouteHandle("abc");
        var h2 = new RouteHandle("abc");
        var h3 = new RouteHandle("xyz");

        Assert.Equal(h1, h2);
        Assert.NotEqual(h1, h3);
    }

    // --- Span ---

    [Fact]
    public void Span_Created()
    {
        var span = new Span(
            new GeoCoordinates(52.5, 13.4),
            new GeoCoordinates(52.6, 13.5),
            LengthInMeters: 1000,
            DurationInSeconds: 60,
            FunctionalRoadClass: 1
        );

        Assert.Equal(52.5, span.Departure.Latitude);
        Assert.Equal(52.6, span.Arrival.Latitude);
        Assert.Equal(1000, span.LengthInMeters);
        Assert.Equal(1, span.FunctionalRoadClass);
    }

    // --- Toll ---

    [Fact]
    public void Toll_Created()
    {
        var toll = new Toll(
            TotalPrice: 5.50m,
            Currency: "EUR",
            CountryCode: "DE"
        );

        Assert.Equal(5.50m, toll.TotalPrice);
        Assert.Equal("EUR", toll.Currency);
        Assert.Equal("DE", toll.CountryCode);
    }

    // --- SectionNotice ---

    [Fact]
    public void SectionNotice_Created()
    {
        var notice = new SectionNotice(
            Code: SectionNoticeCode.TollRoad,
            Severity: NoticeSeverity.Info,
            Text: "Toll road ahead"
        );

        Assert.Equal(SectionNoticeCode.TollRoad, notice.Code);
        Assert.Equal(NoticeSeverity.Info, notice.Severity);
        Assert.Equal("Toll road ahead", notice.Text);
    }

    // --- RoutePlace ---

    [Fact]
    public void RoutePlace_Created()
    {
        var place = new RoutePlace(
            new GeoCoordinates(52.5, 13.4),
            Name: "Berlin Central Station",
            Type: RoutePlaceType.Station
        );

        Assert.Equal(52.5, place.Coordinates.Latitude);
        Assert.Equal("Berlin Central Station", place.Name);
        Assert.Equal(RoutePlaceType.Station, place.Type);
    }

    // --- Signpost ---

    [Fact]
    public void Signpost_Created()
    {
        var signpost = new Signpost(
            RoadNumber: "A100",
            RoadName: "Stadtring",
            ExitNumber: "15",
            Toward: "Dresden"
        );

        Assert.Equal("A100", signpost.RoadNumber);
        Assert.Equal("Dresden", signpost.Toward);
    }

    // --- Enums ---

    [Fact]
    public void SectionNoticeCode_Values()
    {
        Assert.Equal(0, (int)SectionNoticeCode.Unknown);
        Assert.Equal(1, (int)SectionNoticeCode.TollRoad);
        Assert.Equal(2, (int)SectionNoticeCode.RestrictedArea);
    }

    [Fact]
    public void NoticeSeverity_Values()
    {
        Assert.Equal(0, (int)NoticeSeverity.Unknown);
        Assert.Equal(1, (int)NoticeSeverity.Info);
        Assert.Equal(2, (int)NoticeSeverity.Warning);
    }

    [Fact]
    public void RoutePlaceType_Values()
    {
        Assert.Equal(0, (int)RoutePlaceType.Unknown);
        Assert.Equal(1, (int)RoutePlaceType.Station);
        Assert.Equal(2, (int)RoutePlaceType.Parking);
    }
}