using Here.Explore.Maui.Models.Routing;

namespace Here.Explore.Maui.RefApp.Converters;

public class ManeuverActionToIconConverter : IValueConverter
{
    // Material Icons codepoints (rendered with the MaterialIcons font family).
    private const string Depart     = "\ue55d";   // navigation
    private const string Arrive     = "\ue153";   // flag
    private const string Left       = "\ueba6";   // turn_left
    private const string Right      = "\uebab";   // turn_right
    private const string SlightLeft = "\ueba4";   // turn_slight_left
    private const string SlightRight= "\ueb9a";   // turn_slight_right
    private const string Straight   = "\ueb95";   // straight
    private const string UTurn      = "\ue5d5";   // refresh (circular arrow)
    private const string Roundabout = "\ue39e";   // adjust (circle)
    private const string Unknown    = "\ue55d";   // navigation

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not ManeuverAction action) return Unknown;
        return action switch
        {
            ManeuverAction.Depart     => Depart,
            ManeuverAction.Arrive     => Arrive,
            ManeuverAction.Left       => Left,
            ManeuverAction.Right      => Right,
            ManeuverAction.SharpLeft  => SlightLeft,
            ManeuverAction.SlightLeft => SlightLeft,
            ManeuverAction.SharpRight => SlightRight,
            ManeuverAction.SlightRight=> SlightRight,
            ManeuverAction.Straight   => Straight,
            ManeuverAction.UTurnLeft  => UTurn,
            ManeuverAction.UTurnRight => UTurn,
            ManeuverAction.LeftRamp   => Left,
            ManeuverAction.RightRamp  => Right,
            ManeuverAction.LeftExit   => SlightLeft,
            ManeuverAction.RightExit  => SlightRight,
            ManeuverAction.Roundabout => Roundabout,
            ManeuverAction.ContinueOn => Straight,
            _                         => Unknown
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}