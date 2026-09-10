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
        if (value is not TrafficIncidentImpact impact) return Color.FromArgb("#8E8E93");
        return impact switch
        {
            TrafficIncidentImpact.Closed => Color.FromArgb("#E33B4E"),
            TrafficIncidentImpact.Major => Color.FromArgb("#F5A623"),
            TrafficIncidentImpact.Moderate => Color.FromArgb("#F5C518"),
            TrafficIncidentImpact.Minor => Color.FromArgb("#2FBF71"),
            _ => Color.FromArgb("#8E8E93"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}