using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Traffic;
using Here.Explore.Maui.Services;
using Here.Explore.Maui.Models.Maps;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class TrafficViewModel : ViewModelBase
{
    private readonly ITrafficService _trafficService;
    private string _statusMessage = string.Empty;
    private IReadOnlyList<TrafficFlow>? _flows;
    private IReadOnlyList<TrafficIncident>? _incidents;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public IReadOnlyList<TrafficFlow>? Flows
    {
        get => _flows;
        private set => SetProperty(ref _flows, value);
    }

    public IReadOnlyList<TrafficIncident>? Incidents
    {
        get => _incidents;
        private set => SetProperty(ref _incidents, value);
    }

    public TrafficViewModel(ITrafficService trafficService)
    {
        _trafficService = trafficService;
    }

    private ICommand? _queryFlowCommand;
    public ICommand QueryFlowCommand => _queryFlowCommand ??= new Command(async () => await QueryFlowAsync());

    private ICommand? _queryIncidentsCommand;
    public ICommand QueryIncidentsCommand => _queryIncidentsCommand ??= new Command(async () => await QueryIncidentsAsync());

    private async Task QueryFlowAsync()
    {
        StatusMessage = "Querying traffic flow...";
        try
        {
            var area = new GeoCircle(new GeoCoordinates(52.531268, 13.387659), 5000);
            var result = await _trafficService.QueryFlowAsync(area, new TrafficFlowQueryOptions());
            Flows = result.Flows;
            StatusMessage = result.Error == TrafficQueryError.None
                ? $"Found {result.Flows?.Count ?? 0} traffic flow segments"
                : $"Error: {result.Error}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task QueryIncidentsAsync()
    {
        StatusMessage = "Querying traffic incidents...";
        try
        {
            var area = new GeoCircle(new GeoCoordinates(52.531268, 13.387659), 5000);
            var result = await _trafficService.QueryIncidentsAsync(area, new TrafficIncidentsQueryOptions());
            Incidents = result.Incidents;
            StatusMessage = result.Error == TrafficQueryError.None
                ? $"Found {result.Incidents?.Count ?? 0} traffic incidents"
                : $"Error: {result.Error}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}