namespace Here.Explore.Maui.RefApp.Controls;

public partial class CategoryChipBar : ContentView
{
    public record CategoryChip(string CategoryId, string Label, string Icon);

    public event EventHandler<CategoryChip>? CategorySelected;

    private CategoryChip? _selectedChip;
    private readonly List<Border> _chipBorders = new();

    public static readonly CategoryChip[] DefaultCategories =
    {
        new("restaurant", "Restaurants", "🍽"),
        new("hotel", "Hotels", "🏨"),
        new("fuel-station", "Gas Stations", "⛽"),
        new("parking", "Parking", "🅿"),
        new("atm", "ATMs", "🏧"),
        new("hospital", "Hospitals", "🏥"),
        new("shopping", "Shopping", "🛍"),
        new("attraction", "Attractions", "🎯"),
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
