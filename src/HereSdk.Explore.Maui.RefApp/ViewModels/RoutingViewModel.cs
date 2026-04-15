using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Routing;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class RoutingViewModel : ViewModelBase
{
    private readonly IRoutingService? _routingService;
    private Route? _calculatedRoute;
    private string _statusMessage = string.Empty;

    public GeoCoordinates Start { get; set; } = new(52.531268, 13.387659); // Berlin
    public GeoCoordinates End { get; set; } = new(48.8566, 2.3522); // Paris

    public Route? CalculatedRoute
    {
        get => _calculatedRoute;
        private set => SetProperty(ref _calculatedRoute, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RoutingViewModel() { }

    public RoutingViewModel(IRoutingService routingService)
    {
        _routingService = routingService;
    }

    public ICommand CalculateRouteCommand => new Command(async () => await CalculateRouteAsync());

    private async Task CalculateRouteAsync()
    {
        if (_routingService is null) return;

        StatusMessage = "Calculating route...";
        try
        {
            var waypoints = new List<Waypoint>
            {
                new(Start),
                new(End)
            };
            var options = new RoutingOptions();
            var result = await _routingService.CalculateRouteAsync(waypoints, options);

            if (result.Error == RoutingError.None && result.Routes?.Count > 0)
            {
                CalculatedRoute = result.Routes[0];
                StatusMessage = $"Route found: {CalculatedRoute.LengthInMeters / 1000.0:F1} km, {CalculatedRoute.DurationInSeconds / 60:F0} min";
            }
            else
            {
                StatusMessage = $"No route found. Error: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}