using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Maps a <see cref="TrafficIncidentType"/> to a glyph used in the
/// traffic incident list. Gives the user a quick visual signal of
/// what kind of incident each row reports — without this every
/// incident shows the same ⚠ and the list is hard to scan.
/// </summary>
public class TrafficIncidentTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not TrafficIncidentType type) return "⚠";
        return type switch
        {
            TrafficIncidentType.Accident        => "💥",
            TrafficIncidentType.Congestion      => "🚗",
            TrafficIncidentType.Construction    => "🚧",
            TrafficIncidentType.RoadClosure     => "⛔",
            TrafficIncidentType.RoadHazard      => "⚠",
            TrafficIncidentType.DisabledVehicle => "🔧",
            TrafficIncidentType.LaneRestriction => "🚦",
            TrafficIncidentType.MassTransit     => "🚌",
            TrafficIncidentType.PlannedEvent    => "🎉",
            TrafficIncidentType.Weather         => "🌧",
            _                                  => "⚠",
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
