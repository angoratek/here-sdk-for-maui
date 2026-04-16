using System.Windows.Input;
using Here.Explore.Maui.Models;
using Here.Explore.Maui.Models.Maps;
using Here.Explore.Maui.Services;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class MapViewModel : ViewModelBase
{
    private IMapService? _mapService;
    private GeoCoordinates _mapCenter = new(52.531268, 13.387659); // Berlin

    public GeoCoordinates MapCenter
    {
        get => _mapCenter;
        set => SetProperty(ref _mapCenter, value);
    }

    public MapViewModel() { }

    public MapViewModel(IMapService mapService)
    {
        _mapService = mapService;
    }

    /// <summary>
    /// Sets the map service. Called by the page after the handler creates the MapService.
    /// </summary>
    internal void SetMapService(IMapService mapService) => _mapService = mapService;

    private ICommand? _normalDayCommand;
    public ICommand NormalDayCommand => _normalDayCommand ??= new Command(async () =>
    {
        if (_mapService is not null)
            await _mapService.LoadSceneAsync(MapScheme.NormalDay);
    });

    private ICommand? _satelliteCommand;
    public ICommand SatelliteCommand => _satelliteCommand ??= new Command(async () =>
    {
        if (_mapService is not null)
            await _mapService.LoadSceneAsync(MapScheme.SatelliteDay);
    });

    private ICommand? _searchCommand;
    public ICommand SearchCommand => _searchCommand ??= new Command(async () =>
    {
        if (_mapService is not null)
            await _mapService.SetCameraTargetAsync(MapCenter, 10);
    });
}