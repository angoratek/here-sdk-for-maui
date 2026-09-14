namespace Here.Explore.Maui.RefApp.Converters;

public class BoolToOpenColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is true
            ? TokenColor.Get("SuccessGreen", Color.FromArgb("#2FBF71"))
            : TokenColor.Get("ErrorRed", Color.FromArgb("#E33B4E"));

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => Binding.DoNothing;
}