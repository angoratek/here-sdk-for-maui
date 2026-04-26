using System.Globalization;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Returns true if the string value is not null and not empty.
/// Parameter "Invert" reverses the result.
/// </summary>
public class NullOrEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = value is string s && !string.IsNullOrEmpty(s);
        if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            return !result;
        return result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
