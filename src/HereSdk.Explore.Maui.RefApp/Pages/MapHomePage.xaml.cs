using Here.Explore.Maui.RefApp.Panels;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using Microsoft.Maui.Graphics;

namespace Here.Explore.Maui.RefApp.Pages;

/// <summary>
/// The single home page: one shared HereMapView with the four overlay
/// panels (Explore, Directions, Traffic, Tools) swapped on top, plus a
/// custom bottom tab bar. Replaces the former four Shell tabs that each
/// created their own private map.
/// </summary>
public partial class MapHomePage : ContentPage
{
    private readonly PanelNavigationService _panelNavigation;
    private readonly ExploreViewModel _explore;
    private readonly DirectionsViewModel _directions;
    private readonly TrafficViewModel _traffic;
    private readonly ToolsViewModel _tools;
    private readonly ExplorePanel _explorePanel;
    private readonly DirectionsPanel _directionsPanel;
    private readonly TrafficPanel _trafficPanel;
    private readonly ToolsPanel _toolsPanel;
    private Color _selectedColor = Colors.Blue;
    private Color _unselectedColor = Colors.Gray;
    private bool _mapWired;

    public MapHomePage(
        ExploreViewModel explore,
        DirectionsViewModel directions,
        TrafficViewModel traffic,
        ToolsViewModel tools,
        PanelNavigationService panelNavigation)
    {
        InitializeComponent();
        _explore = explore;
        _directions = directions;
        _traffic = traffic;
        _tools = tools;
        _panelNavigation = panelNavigation;

        // Panels are created here (not in XAML): XAML instantiation has no
        // DI, and each panel takes its view-model in the constructor.
        _explorePanel = new ExplorePanel(explore);
        _directionsPanel = new DirectionsPanel(directions);
        _trafficPanel = new TrafficPanel(traffic);
        _toolsPanel = new ToolsPanel(tools);
        _directionsPanel.IsVisible = false;
        _trafficPanel.IsVisible = false;
        _toolsPanel.IsVisible = false;
        PanelHost.Children.Add(_explorePanel);
        PanelHost.Children.Add(_directionsPanel);
        PanelHost.Children.Add(_trafficPanel);
        PanelHost.Children.Add(_toolsPanel);

        // Tab taps wired in code-behind with per-tab closures.
        ExploreTab.Clicked += (_, _) => _panelNavigation.SwitchTo(HomeTab.Explore);
        DirectionsTab.Clicked += (_, _) => _panelNavigation.SwitchTo(HomeTab.Directions);
        TrafficTab.Clicked += (_, _) => _panelNavigation.SwitchTo(HomeTab.Traffic);
        ToolsTab.Clicked += (_, _) => _panelNavigation.SwitchTo(HomeTab.Tools);

        _selectedColor = GetResourceColor("TabSelected") ?? _selectedColor;
        _unselectedColor = GetResourceColor("TabUnselected") ?? _unselectedColor;

        _panelNavigation.TabChanged += (_, tab) => ApplyTab(tab);
        _panelNavigation.DirectionsRequested += (_, place) =>
        {
            // Route through SwitchTo (not ApplyTab directly): the service's
            // ActiveTab must track the visible panel, or a later
            // SwitchTo(HomeTab.Explore) would early-return as "same tab"
            // and the Explore panel could never be shown again.
            _panelNavigation.SwitchTo(HomeTab.Directions);
            _ = _directions.SetPoiDestinationAsync(place);
        };

        MapView.HandlerChanged += OnMapViewHandlerChanged;
        ApplyTab(_panelNavigation.ActiveTab);
    }

    private void OnMapViewHandlerChanged(object? sender, EventArgs e)
    {
        if (_mapWired || MapView.Handler is null || MapView.Map is null) return;
        _mapWired = true;

        // One map, four view-models: every panel operates on the same
        // MapService, so objects drawn on Tools are visible on Explore, etc.
        _explore.SetMapService(MapView.Map);
        _directions.SetMapService(MapView.Map);
        _traffic.SetMapService(MapView.Map);
        _tools.SetMapService(MapView.Map);

        // Reset Map (Tools panel) also clears route, traffic overlays and search.
        _tools.ResetExtras = () =>
        {
            _directions.ClearRouteCommand.Execute(null);
            _traffic.ResetTraffic();
            _explore.ClearSearchCommand.Execute(null);
        };
    }

    /// <summary>
    /// Shows the active panel, gates map-tap handling on the inactive VMs
    /// and repaints the tab bar.
    /// </summary>
    private void ApplyTab(HomeTab tab)
    {
        _explorePanel.IsVisible = tab == HomeTab.Explore;
        _directionsPanel.IsVisible = tab == HomeTab.Directions;
        _trafficPanel.IsVisible = tab == HomeTab.Traffic;
        _toolsPanel.IsVisible = tab == HomeTab.Tools;

        _explore.IsActive = tab == HomeTab.Explore;
        _directions.IsActive = tab == HomeTab.Directions;
        _traffic.IsActive = tab == HomeTab.Traffic;
        _tools.IsActive = tab == HomeTab.Tools;

        // A panel that just lost focus must not leave its suggestion
        // dropdown hovering over the shared map while another panel is
        // active — close it as part of the switch.
        if (tab != HomeTab.Explore) _explore.DismissSuggestions();
        if (tab != HomeTab.Directions) _directions.DismissSuggestions();

        PaintTab(ExploreTab, TabGlyphs.Explore, tab == HomeTab.Explore);
        PaintTab(DirectionsTab, TabGlyphs.Directions, tab == HomeTab.Directions);
        PaintTab(TrafficTab, TabGlyphs.Traffic, tab == HomeTab.Traffic);
        PaintTab(ToolsTab, TabGlyphs.Tools, tab == HomeTab.Tools);
    }

    private void PaintTab(Button tab, string glyph, bool selected)
    {
        var color = selected ? _selectedColor : _unselectedColor;
        tab.TextColor = color;
        tab.ImageSource = new FontImageSource
        {
            Glyph = glyph,
            FontFamily = "MaterialIcons",
            Size = 22,
            Color = color
        };
    }

    private static class TabGlyphs
    {
        public const string Explore = "";
        public const string Directions = "";
        public const string Traffic = "";
        public const string Tools = "";
    }

    private Color? GetResourceColor(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color c)
            return c;
        return null;
    }
}