using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.Controls;

/// <summary>
/// Stub implementation of HereMapView for unit testing.
/// </summary>
public class HereMapView
{
    public HereMapView(IMapService? mapService = null)
    {
        Map = mapService;
    }

    public IMapService Map { get; }
}
