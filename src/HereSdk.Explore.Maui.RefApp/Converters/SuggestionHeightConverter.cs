using System.Collections;
using System.Globalization;

namespace Here.Explore.Maui.RefApp.Converters;

/// <summary>
/// Sizes a suggestion <c>CollectionView</c> to its content instead of a fixed
/// height: <c>ItemsSource="{Binding X}" HeightRequest="{Binding X, Converter=...}"</c>.
/// The parameter is "rowHeight,maxHeight" in device pixels — the list grows
/// with the item count up to the cap, then scrolls internally.
/// </summary>
public class SuggestionHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value is ICollection collection ? collection.Count : 0;
        var parts = (parameter as string)?.Split(',') ?? new[] { "54", "240" };
        var rowHeight = double.Parse(parts[0], CultureInfo.InvariantCulture);
        var maxHeight = double.Parse(parts[1], CultureInfo.InvariantCulture);
        return Math.Min(count * rowHeight, maxHeight);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}