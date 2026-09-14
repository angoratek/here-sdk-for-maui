using Here.Explore.Maui.Models.Search;

namespace Here.Explore.Maui.RefApp.Services;

/// <summary>Identifies the overlay panel shown over the shared map.</summary>
public enum HomeTab
{
    Explore,
    Directions,
    Traffic,
    Tools
}

/// <summary>
/// Coordinates the single-map home page: which overlay panel is active, and
/// requests to open Directions for a place (formerly a Shell route with
/// query attributes — with one shared map there is no second page to
/// navigate to, the Directions panel just becomes active).
/// </summary>
public class PanelNavigationService
{
    public HomeTab ActiveTab { get; private set; } = HomeTab.Explore;

    /// <summary>Raised when the active tab changed; payload is the new tab.</summary>
    public event EventHandler<HomeTab>? TabChanged;

    /// <summary>Raised when a place card asks to open Directions.</summary>
    public event EventHandler<Place>? DirectionsRequested;

    public void SwitchTo(HomeTab tab)
    {
        if (ActiveTab == tab) return;
        ActiveTab = tab;
        TabChanged?.Invoke(this, tab);
    }

    public void RequestDirections(Place place)
    {
        DirectionsRequested?.Invoke(this, place);
    }
}