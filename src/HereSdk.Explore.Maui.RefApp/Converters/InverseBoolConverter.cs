namespace Here.Explore.Maui.RefApp.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is bool b ? !b : value;
}
