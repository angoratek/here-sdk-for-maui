using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.RefApp.Controls;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class TrafficViewModel : ViewModelBase
{
    private readonly ITrafficService _trafficService;
    private IMapService? _mapService;

    [ObservableProperty] private bool _isFlowVisible;
    [ObservableProperty] private bool _isIncidentsVisible = true;
    [ObservableProperty] private IReadOnlyList<TrafficFlow> _flows = Array.Empty<TrafficFlow>();
    [ObservableProperty] private IReadOnlyList<TrafficIncident> _incidents = Array.Empty<TrafficIncident>();
    [ObservableProperty] private TrafficIncident? _selectedIncident;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "";
    // Success/status notifications ("N incidents found") render as a green
    // info banner; only real failures render red.
    [ObservableProperty] private BannerSeverity _statusSeverity = BannerSeverity.Info;
    [ObservableProperty] private int _incidentCount;
    [ObservableProperty] private int _flowCount;
    [ObservableProperty] private string? _emptyStateTitle;
    [ObservableProperty] private string? _emptyStateSubtitle;

    private readonly List<MapPolyline> _flowPolylines = new();
    private readonly List<MapMarker> _incidentMarkers = new();
    private MapMarker? _selectedMarker;
    private GeoCircle _lastQueryArea = new(new GeoCoordinates(52.531268, 13.387659), 5000);

    public TrafficViewModel(ITrafficService trafficService)
    {
        _trafficService = trafficService;
    }

    /// <summary>
    /// Whether this VM's tab panel is the active one over the shared map.
    /// Defaults to true so unit tests that never switch tabs behave as
    /// before. (Traffic currently has no map-tap handling; kept for parity
    /// with the other panel VMs.)
    /// </summary>
    public bool IsActive { get; set; } = true;

    public void SetMapService(IMapService mapService)
    {
        _mapService = mapService;
    }

    [RelayCommand]
    private async Task ToggleFlow()
    {
        IsFlowVisible = !IsFlowVisible;
        if (IsFlowVisible)
            await QueryFlowAsync();
        else
            ClearFlows();
    }

    [RelayCommand]
    private async Task ToggleIncidents()
    {
        IsIncidentsVisible = !IsIncidentsVisible;
        if (IsIncidentsVisible)
            await QueryIncidentsAsync();
        else
            ClearIncidents();
    }

    [RelayCommand]
    private async Task RefreshTraffic()
    {
        if (_mapService is not null)
        {
            var center = await _mapService.GetCameraTargetAsync();
            _lastQueryArea = new GeoCircle(center, 5000);
        }

        if (IsFlowVisible) await QueryFlowAsync();
        if (IsIncidentsVisible) await QueryIncidentsAsync();
    }

    [RelayCommand]
    private async Task SelectIncident(TrafficIncident incident)
    {
        SelectedIncident = incident;

        // Look up full details
        try
        {
            var detail = await _trafficService.LookupIncidentAsync(incident.Id, new TrafficIncidentLookupOptions());
            if (detail is not null)
                SelectedIncident = detail;
        }
        catch { /* keep original */ }

        // Highlight on map (if we had geometry, but TrafficIncident doesn't expose it in our API)
        if (_mapService is not null)
        {
            if (_selectedMarker is not null)
                _mapService.RemoveMapMarker(_selectedMarker);
            // Show info marker at center
            _selectedMarker = new MapMarker(_lastQueryArea.Center, Color: MarkerVisuals.Neutral, Glyph: MarkerVisuals.GlyphPlace);
            _mapService.AddMapMarker(_selectedMarker);
        }
    }

    private async Task QueryFlowAsync()
    {
        IsLoading = true;
        ClearFlows();
        EmptyStateTitle = null;
        try
        {
            var result = await _trafficService.QueryFlowAsync(_lastQueryArea, new TrafficFlowQueryOptions());
            if (result.Flows is not null && result.Error == TrafficQueryError.None)
            {
                Flows = result.Flows;
                FlowCount = result.Flows.Count;

                if (FlowCount == 0)
                {
                    EmptyStateTitle = "No traffic flow data";
                    EmptyStateSubtitle = "Try moving to a busier area";
                    StatusMessage = "";
                    return;
                }

                // Render flow as colored polylines
                if (_mapService is not null)
                {
                    foreach (var flow in result.Flows)
                    {
                        if (flow.Geometry is { Vertices.Count: >= 2 })
                        {
                            var color = JamFactorToColor(flow.JamFactor);
                            var line = new MapPolyline(flow.Geometry.Vertices, Color: color, WidthInPixels: 4);
                            _mapService.AddMapPolyline(line);
                            _flowPolylines.Add(line);
                        }
                    }
                }
                StatusMessage = $"{FlowCount} flow segments rendered";
                StatusSeverity = BannerSeverity.Info;
            }
            else
            {
                StatusMessage = $"Flow query error: {result.Error}";
                StatusSeverity = BannerSeverity.Error;
            }
        }
        catch (Exception ex) { StatusMessage = ex.Message; StatusSeverity = BannerSeverity.Error; }
        finally { IsLoading = false; }
    }

    private async Task QueryIncidentsAsync()
    {
        IsLoading = true;
        ClearIncidents();
        if (EmptyStateTitle is not null && !IsFlowVisible)
            EmptyStateTitle = null;
        try
        {
            var result = await _trafficService.QueryIncidentsAsync(_lastQueryArea, new TrafficIncidentsQueryOptions());
            if (result.Incidents is not null && result.Error == TrafficQueryError.None)
            {
                Incidents = result.Incidents;
                IncidentCount = result.Incidents.Count;

                if (IncidentCount == 0)
                {
                    EmptyStateTitle = "No traffic incidents";
                    EmptyStateSubtitle = "No incidents reported in this area";
                    StatusMessage = "";
                    return;
                }

                // Render incident markers
                if (_mapService is not null)
                {
                    foreach (var incident in result.Incidents)
                    {
                        // Each incident has a Location (first vertex of its
                        // road polyline) populated by the service layer.
                        // The previous implementation used a hash-derived
                        // offset at the query center, so all incident
                        // markers stacked on the same spot and were not
                        // distinguishable from each other on the map.
                        var position = incident.Location ?? _lastQueryArea.Center;
                        var (glyph, tint) = MarkerVisuals.ForIncident(incident);
                        var marker = new MapMarker(position, Color: tint, Glyph: glyph);
                        _mapService.AddMapMarker(marker);
                        _incidentMarkers.Add(marker);
                    }
                }
                StatusMessage = $"{IncidentCount} incidents found";
                StatusSeverity = BannerSeverity.Info;
            }
            else
            {
                StatusMessage = $"Incident query error: {result.Error}";
                StatusSeverity = BannerSeverity.Error;
            }
        }
        catch (Exception ex) { StatusMessage = ex.Message; StatusSeverity = BannerSeverity.Error; }
        finally { IsLoading = false; }
    }

    /// <summary>
    /// Removes all traffic overlays and hides both toggles. Called by the
    /// Tools panel's Reset Map action over the shared map.
    /// </summary>
    public void ResetTraffic()
    {
        IsFlowVisible = false;
        IsIncidentsVisible = false;
        ClearFlows();
        ClearIncidents();
    }

    private void ClearFlows()
    {
        if (_mapService is null) return;
        foreach (var p in _flowPolylines) _mapService.RemoveMapPolyline(p);
        _flowPolylines.Clear();
        FlowCount = 0;
    }

    private void ClearIncidents()
    {
        if (_mapService is null) return;
        foreach (var m in _incidentMarkers) _mapService.RemoveMapMarker(m);
        _incidentMarkers.Clear();
        IncidentCount = 0;
    }

    private static uint JamFactorToColor(double jamFactor) => jamFactor switch
    {
        < 4 => 0xFF34C759,
        < 7 => 0xFFFFCC02,
        _ => 0xFFFF3B30,
    };
}
