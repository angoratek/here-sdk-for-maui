using System.Globalization;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Converts boolean to Color (true = blue tracking color, false = white).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTracking && isTracking)
            return Color.FromArgb("#2196F3"); // Blue when tracking
        return Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
