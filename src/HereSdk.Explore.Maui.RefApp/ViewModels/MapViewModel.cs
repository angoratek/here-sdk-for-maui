using System.Windows.Input;
using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class MapViewModel : ViewModelBase
{
    private GeoCoordinates _mapCenter = new(52.531268, 13.387659); // Berlin

    public GeoCoordinates MapCenter
    {
        get => _mapCenter;
        set => SetProperty(ref _mapCenter, value);
    }

    public ICommand NormalDayCommand => new Command(() => { /* Switch to NormalDay scheme */ });
    public ICommand SatelliteCommand => new Command(() => { /* Switch to Satellite scheme */ });
    public ICommand SearchCommand => new Command(() => { /* Navigate to search */ });
}