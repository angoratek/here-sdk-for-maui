using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Maps a <see cref="TrafficIncidentType"/> to a Material Icons glyph
/// used in the traffic incident list. Gives the user a quick visual
/// signal of what kind of incident each row reports — without this
/// every incident shows the same icon and the list is hard to scan.
/// </summary>
public class TrafficIncidentTypeToIconConverter : IValueConverter
{
    // Material Icons codepoints (rendered with the MaterialIcons font family).
    private const string Accident        = "\ue0c8";  // location_on
    private const string Congestion      = "\ue531";  // directions_car
    private const string Construction    = "\uea3c";  // construction
    private const string RoadClosure     = "\ue5cd";  // close
    private const string RoadHazard      = "\ue002";  // warning
    private const string DisabledVehicle = "\ue0b9";  // build (wrench)
    private const string LaneRestriction = "\ue565";  // traffic
    private const string MassTransit     = "\ue530";  // directions_bus
    private const string PlannedEvent    = "\ue53f";  // local_attraction
    private const string Weather         = "\ue430";  // wb_sunny
    private const string Unknown         = "\ue002";  // warning

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not TrafficIncidentType type) return Unknown;
        return type switch
        {
            TrafficIncidentType.Accident        => "\ue0c8",  // location_on (pin)
            TrafficIncidentType.Congestion      => Congestion,
            TrafficIncidentType.Construction    => Construction,
            TrafficIncidentType.RoadClosure     => RoadClosure,
            TrafficIncidentType.RoadHazard      => RoadHazard,
            TrafficIncidentType.DisabledVehicle => DisabledVehicle,
            TrafficIncidentType.LaneRestriction => LaneRestriction,
            TrafficIncidentType.MassTransit     => MassTransit,
            TrafficIncidentType.PlannedEvent    => "\ue53f",  // local_attraction
            TrafficIncidentType.Weather         => Weather,
            _                                  => Unknown,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}