using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;

#if IOS
namespace Here.Explore.Maui.DeviceTests.iOS;

public class TrafficServiceiOSTests
{
    // iOS TrafficIncidentType raw values: accident=0, congestion=1,
    // construction=2, disabledVehicle=3, massTransit=4, plannedEvent=5,
    // roadHazard=6, weather=7, roadClosure=8, laneRestriction=9, other=10,
    // unknown=11. The raw order differs from the shared enum — a direct
    // cast corrupts the values, so these tests pin the explicit mapping.
    [Theory]
    [InlineData(0, TrafficIncidentType.Accident)]
    [InlineData(1, TrafficIncidentType.Congestion)]
    [InlineData(2, TrafficIncidentType.Construction)]
    [InlineData(3, TrafficIncidentType.DisabledVehicle)]
    [InlineData(4, TrafficIncidentType.MassTransit)]
    [InlineData(5, TrafficIncidentType.PlannedEvent)]
    [InlineData(6, TrafficIncidentType.RoadHazard)]
    [InlineData(7, TrafficIncidentType.Weather)]
    [InlineData(8, TrafficIncidentType.RoadClosure)]
    [InlineData(9, TrafficIncidentType.LaneRestriction)]
    [InlineData(10, TrafficIncidentType.Miscellaneous)]
    [InlineData(11, TrafficIncidentType.Unknown)]
    public void ToSharedIncidentType_MapsRawValue(int rawValue, TrafficIncidentType expected)
    {
        Assert.Equal(expected, TrafficService.ToSharedIncidentType(rawValue));
    }

    // iOS TrafficIncidentImpact raw values: critical=0, major=1, minor=2,
    // low=3, unknown=4. Mapped consistently with Android
    // (Critical → Closed, Low → Minor).
    [Theory]
    [InlineData(0, TrafficIncidentImpact.Closed)]
    [InlineData(1, TrafficIncidentImpact.Major)]
    [InlineData(2, TrafficIncidentImpact.Minor)]
    [InlineData(3, TrafficIncidentImpact.Minor)]
    [InlineData(4, TrafficIncidentImpact.Unknown)]
    public void ToSharedIncidentImpact_MapsRawValue(int rawValue, TrafficIncidentImpact expected)
    {
        Assert.Equal(expected, TrafficService.ToSharedIncidentImpact(rawValue));
    }
}
#endif
