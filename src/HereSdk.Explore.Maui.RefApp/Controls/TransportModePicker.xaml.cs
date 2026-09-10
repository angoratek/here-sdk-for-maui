namespace Here.Explore.Maui.RefApp.Controls;

public partial class TransportModePicker : ContentView
{
    public record TransportModeOption(string Label, string Icon, int ModeKey)
    {
        public bool IsSelected { get; set; }
    }

    public event EventHandler<int>? ModeSelected;

    public static readonly TransportModeOption[] Modes =
    {
        // Icons are Material Icons glyphs (codepoints).
        new("Car", "\ue531", 0),          // directions_car
        new("Truck", "\ue558", 1),        // local_shipping
        new("Pedestrian", "\ue536", 2),   // directions_walk
        new("Bicycle", "\ue52f", 3),      // directions_bike
        new("Scooter", "\ueb1f", 4),      // electric_scooter
        new("Bus", "\ue530", 5),          // directions_bus
        new("Taxi", "\ue559", 6),         // local_taxi
        new("Transit", "\ue571", 7),      // tram
    };

    private readonly List<Border> _chipBorders = new();
    private readonly List<Label> _chipLabels = new();
    private readonly List<Label> _iconLabels = new();
    private int _selectedModeKey;

    public TransportModePicker()
    {
        InitializeComponent();
        Populate();
        SelectMode(0); // Default: Car
    }

    private void Populate()
    {
        foreach (var mode in Modes)
        {
            var content = new HorizontalStackLayout
            {
                Spacing = 7,
                VerticalOptions = LayoutOptions.Center
            };
            var icon = new Label
            {
                Text = mode.Icon,
                FontFamily = "MaterialIcons",
                FontSize = 17,
                VerticalOptions = LayoutOptions.Center
            };
            var label = new Label
            {
                Text = mode.Label,
                FontSize = 13,
                FontFamily = "InterMedium",
                VerticalOptions = LayoutOptions.Center
            };
            content.Children.Add(icon);
            content.Children.Add(label);

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 19 },
                StrokeThickness = 0,
                Padding = new Thickness(13, 8),
                HeightRequest = 40,
                Content = content,
                BackgroundColor = ThemeColor("SurfaceTertiary", "SurfaceTertiaryDark"),
            };

            var modeIndex = mode.ModeKey;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SelectMode(modeIndex);
            border.GestureRecognizers.Add(tap);

            _chipBorders.Add(border);
            _chipLabels.Add(label);
            _iconLabels.Add(icon);
            ModesContainer.Children.Add(border);
        }
    }

    private static Color ThemeColor(string lightKey, string darkKey)
    {
        var app = Application.Current;
        if (app?.Resources.TryGetValue(lightKey, out var light) == true &&
            app.RequestedTheme == AppTheme.Light)
            return (Color)light;
        if (app?.Resources.TryGetValue(darkKey, out var dark) == true)
            return (Color)dark;
        if (app?.Resources.TryGetValue(lightKey, out var fallback) == true)
            return (Color)fallback;
        return Color.FromArgb("#EBEBEB");
    }

    public void SelectMode(int modeKey)
    {
        _selectedModeKey = modeKey;
        var primary = ThemeColor("Primary", "PrimaryDark");
        var textPrimary = ThemeColor("TextPrimary", "TextPrimaryDark");
        var textSecondary = ThemeColor("TextSecondary", "TextSecondaryDark");
        for (int i = 0; i < _chipBorders.Count; i++)
        {
            if (i == modeKey)
            {
                _chipBorders[i].BackgroundColor = primary;
                _chipBorders[i].Stroke = primary;
                _chipLabels[i].TextColor = Colors.White;
                _iconLabels[i].TextColor = Colors.White;
            }
            else
            {
                _chipBorders[i].BackgroundColor = ThemeColor("SurfaceTertiary", "SurfaceTertiaryDark");
                _chipBorders[i].Stroke = _chipBorders[i].BackgroundColor;
                _chipLabels[i].TextColor = textPrimary;
                _iconLabels[i].TextColor = textSecondary;
            }
        }
        ModeSelected?.Invoke(this, modeKey);
    }
}