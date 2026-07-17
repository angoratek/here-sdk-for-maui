namespace Here.Explore.Maui.RefApp.Controls;

public partial class CategoryChipBar : ContentView
{
    public record CategoryChip(string CategoryId, string Label, string Icon);

    public event EventHandler<CategoryChip>? CategorySelected;

    private CategoryChip? _selectedChip;
    private readonly List<Border> _chipBorders = new();

    public static readonly CategoryChip[] DefaultCategories =
    {
        // HERE Place Category IDs from the HERE Places Category System
        // (https://developer.here.com/documentation places API). The IDs are
        // taxonomy codes such as "100-1000" for restaurants; the previous
        // values ("restaurant", "hotel", …) were placeholders and caused
        // the API to return 400 Illegal input for parameter 'categories'.
        new("100-1000", "Restaurants", "🍽"),
        new("500-5000", "Hotels", "🏨"),
        new("700-7600-0116", "Gas Stations", "⛽"),
        new("800-8500", "Parking", "🅿"),
        new("700-7010", "ATMs", "🏧"),
        new("800-8000", "Hospitals", "🏥"),
        new("600", "Shopping", "🛍"),
        new("300", "Attractions", "🎯"),
    };

    public CategoryChipBar()
    {
        InitializeComponent();
        Populate();
    }

    private void Populate()
    {
        foreach (var chip in DefaultCategories)
        {
            var chipLabel = new Label
            {
                Text = $"{chip.Icon} {chip.Label}",
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#000000")
            };
            var chipBorder = new Border
            {
                // Per-chip AutomationId so Appium can target each one
                // without relying on the emoji-prefixed label text.
                AutomationId = $"ExploreCategory{ChipIdFromLabel(chip.Label)}",
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 1,
                Padding = new Thickness(12, 6),
                HeightRequest = 36,
                Content = chipLabel,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5E5EA")
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => OnChipTapped(chip, chipBorder, chipLabel);
            chipBorder.GestureRecognizers.Add(tap);

            _chipBorders.Add(chipBorder);
            ChipsContainer.Children.Add(chipBorder);
        }
    }

    /// <summary>
    /// Maps a chip's human-readable label (e.g. "Restaurants") to the
    /// suffix used in its AutomationId (e.g. "Restaurants"). One-to-one
    /// with the labels in <see cref="DefaultCategories"/>; labels with
    /// spaces are stripped (none currently do).
    /// </summary>
    private static string ChipIdFromLabel(string label) =>
        label.Replace(" ", string.Empty, StringComparison.Ordinal);

    private void OnChipTapped(CategoryChip chip, Border border, Label label)
    {
        // Deselect all
        foreach (var b in _chipBorders)
        {
            b.BackgroundColor = Colors.White;
            b.Stroke = Color.FromArgb("#E5E5EA");
            if (b.Content is Label l)
            {
                l.TextColor = Color.FromArgb("#000000");
            }
        }

        // Select tapped
        border.BackgroundColor = Color.FromArgb("#007AFF");
        border.Stroke = Color.FromArgb("#007AFF");
        label.TextColor = Colors.White;

        _selectedChip = chip;
        CategorySelected?.Invoke(this, chip);
    }
}
