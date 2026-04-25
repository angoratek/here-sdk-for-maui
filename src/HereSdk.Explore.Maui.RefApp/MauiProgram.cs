using Here.Explore.Maui;
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.Services;
using Microsoft.Extensions.Configuration;

namespace Here.Explore.Maui.RefApp;

public static class MauiProgram
{
    /// <summary>Initialization error message (null if init succeeded).</summary>
    public static string? InitError { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<IHereMapView, HereMapViewHandler>();
        });

        // Initialize HERE SDK — errors are captured in InitError for UI display
        InitError = null;
        try
        {
            System.Diagnostics.Debug.WriteLine("DEBUG: CreateMauiApp starting");
            Android.Util.Log.Debug("REFAPP_DIAG", "CreateMauiApp starting");

            using var configStream = OpenAppSettingsStream();
            var config = new ConfigurationBuilder()
                .AddJsonStream(configStream)
                .Build();

            var keyId = config["HereSdk:AccessKeyId"];
            var keySecret = config["HereSdk:AccessKeySecret"];

            Android.Util.Log.Debug("REFAPP_DIAG", $"Credentials loaded: keyId={(string.IsNullOrEmpty(keyId) ? "MISSING" : "PRESENT")}, secret={(string.IsNullOrEmpty(keySecret) ? "MISSING" : "PRESENT")}");

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                InitError = "Credentials missing in appsettings.json";
                Android.Util.Log.Error("REFAPP_DIAG", InitError);
                throw new InvalidOperationException(InitError);
            }

            Android.Util.Log.Debug("REFAPP_DIAG", "Initializing HERE SDK...");
            HereSdk.Initialize(new HereSdkOptions
            {
                AccessKeyId = keyId,
                AccessKeySecret = keySecret
            });
            Android.Util.Log.Debug("REFAPP_DIAG", "HERE SDK initialized successfully");

            builder.Services.AddSingleton<IRoutingService, RoutingService>();
            builder.Services.AddSingleton<ISearchService, SearchService>();
            builder.Services.AddSingleton<ITrafficService, TrafficService>();
        }
        catch (Exception ex)
        {
            InitError ??= $"{ex.GetType().Name}: {ex.Message}";
            Android.Util.Log.Error("REFAPP_DIAG", $"Init error: {InitError}");
            System.Diagnostics.Debug.WriteLine($"INIT ERROR: {InitError}");
        }

        // Register ViewModels (always register so the page can render)
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<RoutingViewModel>();
        builder.Services.AddTransient<TrafficViewModel>();
        builder.Services.AddTransient<MapItemsViewModel>();

        Android.Util.Log.Debug("REFAPP_DIAG", "CreateMauiApp completed");
        return builder.Build();
    }

    private static Stream OpenAppSettingsStream()
    {
        // Read from embedded resource — works without any Android/iOS context
        var assembly = typeof(MauiProgram).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames
            .FirstOrDefault(n => n.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new InvalidOperationException(
                $"Embedded resource appsettings.json not found. Available: [{string.Join(", ", resourceNames)}]");

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Failed to load embedded resource: {resourceName}");
    }
}