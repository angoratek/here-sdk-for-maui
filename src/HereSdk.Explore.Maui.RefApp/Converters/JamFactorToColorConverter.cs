namespace Here.Explore.Maui.RefApp.Converters;

public class JamFactorToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not double jamFactor) return Color.FromArgb("#34C759");
        return jamFactor switch
        {
            < 4 => Color.FromArgb("#34C759"),   // Free flow
            < 7 => Color.FromArgb("#FFCC02"),   // Moderate
            _   => Color.FromArgb("#FF3B30"),   // Heavy
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
