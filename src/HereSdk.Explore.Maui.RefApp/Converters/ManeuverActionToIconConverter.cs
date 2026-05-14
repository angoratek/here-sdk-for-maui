using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.RefApp.Converters;

public class ManeuverActionToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not ManeuverAction action) return "↗";
        return action switch
        {
            ManeuverAction.Depart     => "▶",
            ManeuverAction.Arrive     => "🏁",
            ManeuverAction.Left       => "↰",
            ManeuverAction.Right      => "↱",
            ManeuverAction.SharpLeft  => "⬅",
            ManeuverAction.SlightLeft => "↖",
            ManeuverAction.SharpRight => "➡",
            ManeuverAction.SlightRight=> "↗",
            ManeuverAction.Straight   => "↑",
            ManeuverAction.UTurnLeft  => "↩",
            ManeuverAction.UTurnRight => "↪",
            ManeuverAction.LeftRamp   => "🚗⬅",
            ManeuverAction.RightRamp  => "🚗➡",
            ManeuverAction.LeftExit   => "⬅🚗",
            ManeuverAction.RightExit  => "➡🚗",
            ManeuverAction.Roundabout => "🔄",
            ManeuverAction.ContinueOn => "↑",
            _                         => "↗"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
