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
        new("Car", "🚗", 0),
        new("Truck", "🚛", 1),
        new("Pedestrian", "🚶", 2),
        new("Bicycle", "🚲", 3),
        new("Scooter", "🛴", 4),
        new("Bus", "🚌", 5),
        new("Taxi", "🚕", 6),
        new("Transit", "🚊", 7),
    };

    private readonly List<Border> _chipBorders = new();
    private readonly List<Label> _chipLabels = new();
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
            var label = new Label
            {
                Text = $"{mode.Icon} {mode.Label}",
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center
            };
            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 },
                StrokeThickness = 1,
                Padding = new Thickness(14, 8),
                HeightRequest = 40,
                Content = label,
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E5E5EA"),
            };

            var modeIndex = mode.ModeKey;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SelectMode(modeIndex);
            border.GestureRecognizers.Add(tap);

            _chipBorders.Add(border);
            _chipLabels.Add(label);
            ModesContainer.Children.Add(border);
        }
    }

    public void SelectMode(int modeKey)
    {
        _selectedModeKey = modeKey;
        for (int i = 0; i < _chipBorders.Count; i++)
        {
            if (i == modeKey)
            {
                _chipBorders[i].BackgroundColor = Color.FromArgb("#007AFF");
                _chipBorders[i].Stroke = Color.FromArgb("#007AFF");
                _chipLabels[i].TextColor = Colors.White;
            }
            else
            {
                _chipBorders[i].BackgroundColor = Colors.White;
                _chipBorders[i].Stroke = Color.FromArgb("#E5E5EA");
                _chipLabels[i].TextColor = Color.FromArgb("#000000");
            }
        }
        ModeSelected?.Invoke(this, modeKey);
    }
}
