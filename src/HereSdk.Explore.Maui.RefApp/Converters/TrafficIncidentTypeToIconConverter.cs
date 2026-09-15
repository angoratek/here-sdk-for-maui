using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.RefApp.Services;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Maps a <see cref="TrafficIncidentType"/> to a Material Icons glyph
/// used in the traffic incident list. Delegates to
/// <see cref="MarkerVisuals.IncidentGlyph"/> so the list icons and the
/// incident map pins stay in one visual language.
/// </summary>
public class TrafficIncidentTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => MarkerVisuals.IncidentGlyph(
            value is TrafficIncidentType type ? type : TrafficIncidentType.Miscellaneous);

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}