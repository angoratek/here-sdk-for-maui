using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class MainPage : ContentPage
{
    private MapViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _viewModel = new MapViewModel();
        BindingContext = _viewModel;

        MapView.HandlerChanged += (s, e) =>
        {
            try
            {
                var handler = MapView.Handler;
                Android.Util.Log.Debug("REFAPP_DIAG", $"HandlerChanged: handler={(handler is null ? "null" : "ok")}");

                if (handler is null)
                {
                    StatusLabel.Text = "Handler is null";
                    StatusLabel.TextColor = Colors.Red;
                    return;
                }

                var h = (HereMapViewHandler)handler;
                Android.Util.Log.Debug("REFAPP_DIAG", $"Handler type: {h.GetType().Name}");

                if (h.MapService is { } mapService)
                {
                    Android.Util.Log.Debug("REFAPP_DIAG", $"MapService found, wiring MapIdle event");
                    StatusLabel.Text = "Loading scene...";
                    StatusLabel.TextColor = Colors.DodgerBlue;
                    _viewModel.SetMapService(mapService);

                    mapService.MapIdle += (_, _) =>
                    {
                        Android.Util.Log.Debug("REFAPP_DIAG", "MapIdle event fired!");
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            StatusLabel.Text = "Map loaded";
                            StatusLabel.TextColor = Colors.Green;
                            Android.Util.Log.Debug("REFAPP_DIAG", "StatusLabel updated to 'Map loaded'");
                        });
                    };
                }
                else
                {
                    // MapService is null — show what went wrong
                    var err = MauiProgram.InitError;
                    Android.Util.Log.Debug("REFAPP_DIAG", $"MapService is null, InitError={err}");
                    StatusLabel.Text = err ?? $"SDK not initialized (IsInit={HereSdk.IsInitialized})";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Debug("REFAPP_DIAG", $"HandlerChanged exception: {ex}");
                StatusLabel.Text = $"Err: {ex.GetType().Name}: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
            }
        };
    }
}