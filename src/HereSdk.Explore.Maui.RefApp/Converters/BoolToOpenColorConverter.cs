namespace Here.Explore.Maui.RefApp.Converters;

public class BoolToOpenColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is true ? Color.FromArgb("#34C759") : Color.FromArgb("#FF3B30");

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}
