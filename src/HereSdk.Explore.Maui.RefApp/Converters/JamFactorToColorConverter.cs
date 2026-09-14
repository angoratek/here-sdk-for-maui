namespace Here.Explore.Maui.RefApp.Converters;

public class JamFactorToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not double jamFactor) return TokenColor.Get("TrafficFreeFlow", Color.FromArgb("#2FBF71"));
        return jamFactor switch
        {
            < 4 => TokenColor.Get("TrafficFreeFlow", Color.FromArgb("#2FBF71")),
            < 7 => TokenColor.Get("TrafficModerate", Color.FromArgb("#F5C518")),   // Moderate
            _   => TokenColor.Get("TrafficHeavy", Color.FromArgb("#FF385C")),      // Heavy
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}