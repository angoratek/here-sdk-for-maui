using Here.Explore.Maui.Models.Traffic;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Maps a <see cref="TrafficIncidentImpact"/> to the severity-rail color
/// shown on the incident card's left edge.
/// </summary>
public class TrafficIncidentImpactToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not TrafficIncidentImpact impact)
            return TokenColor.Get("TabUnselected", Color.FromArgb("#8E8E93"));
        return impact switch
        {
            TrafficIncidentImpact.Closed => TokenColor.Get("ErrorRed", Color.FromArgb("#E33B4E")),
            TrafficIncidentImpact.Major => TokenColor.Get("WarningOrange", Color.FromArgb("#F5A623")),
            TrafficIncidentImpact.Moderate => TokenColor.Get("TrafficModerate", Color.FromArgb("#F5C518")),
            TrafficIncidentImpact.Minor => TokenColor.Get("SuccessGreen", Color.FromArgb("#2FBF71")),
            _ => TokenColor.Get("TabUnselected", Color.FromArgb("#8E8E93")),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}