using Here.Explore.Maui;
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.RefApp.Converters;
using Here.Explore.Maui.RefApp.Pages;
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

            using var configStream = OpenAppSettingsStream();
            var config = new ConfigurationBuilder()
                .AddJsonStream(configStream)
                .Build();

            var keyId = config["HereSdk:AccessKeyId"];
            var keySecret = config["HereSdk:AccessKeySecret"];

            System.Diagnostics.Debug.WriteLine($"Credentials loaded: keyId={(string.IsNullOrEmpty(keyId) ? "MISSING" : "PRESENT")}, secret={(string.IsNullOrEmpty(keySecret) ? "MISSING" : "PRESENT")}");

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                InitError = "Credentials missing in appsettings.json";
                System.Diagnostics.Debug.WriteLine($"INIT ERROR: {InitError}");
                throw new InvalidOperationException(InitError);
            }

            System.Diagnostics.Debug.WriteLine("Initializing HERE SDK...");
            HereSdk.Initialize(new HereSdkOptions
            {
                AccessKeyId = keyId,
                AccessKeySecret = keySecret
            });
            System.Diagnostics.Debug.WriteLine("HERE SDK initialized successfully");

            builder.Services.AddSingleton<IRoutingService, RoutingService>();
            builder.Services.AddSingleton<ISearchService, SearchService>();
            builder.Services.AddSingleton<ITrafficService, TrafficService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
        }
        catch (Exception ex)
        {
            InitError ??= $"{ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Init error: {InitError}");
            System.Diagnostics.Debug.WriteLine($"INIT ERROR: {InitError}");
        }

        // Register ViewModels
        builder.Services.AddTransient<MapViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<RoutingViewModel>();
        builder.Services.AddTransient<TrafficViewModel>();
        builder.Services.AddTransient<MapItemsViewModel>();
        builder.Services.AddTransient<ModernMainViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register pages
        builder.Services.AddTransient<ModernMainPage>();
        builder.Services.AddTransient<SettingsPage>();

        // Register converters
        builder.Services.AddTransient<NullToBoolConverter>();
        builder.Services.AddTransient<BoolToColorConverter>();

        System.Diagnostics.Debug.WriteLine("CreateMauiApp completed");
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