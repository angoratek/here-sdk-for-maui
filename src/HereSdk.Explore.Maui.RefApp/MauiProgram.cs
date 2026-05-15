using Here.Explore.Maui;
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.RefApp.Converters;
using Here.Explore.Maui.RefApp.Pages;
using Here.Explore.Maui.RefApp.Services;
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

        // Load configuration
        using var configStream = OpenAppSettingsStream();
        var config = new ConfigurationBuilder()
            .AddJsonStream(configStream)
            .Build();
        builder.Configuration.AddConfiguration(config);

        // Initialize HERE SDK — errors are captured in InitError for UI display
        InitError = null;
        try
        {
            var keyId = config["HereSdk:AccessKeyId"];
            var keySecret = config["HereSdk:AccessKeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                InitError = "Credentials missing in appsettings.json";
                throw new InvalidOperationException(InitError);
            }

            HereSdk.Initialize(new HereSdkOptions
            {
                AccessKeyId = keyId,
                AccessKeySecret = keySecret
            });

            builder.Services.AddSingleton<IRoutingService, RoutingService>();
            builder.Services.AddSingleton<ISearchService, SearchService>();
            builder.Services.AddSingleton<ITrafficService, TrafficService>();
            builder.Services.AddSingleton<ILocationService, LocationService>();
        }
        catch (Exception ex)
        {
            InitError ??= $"{ex.GetType().Name}: {ex.Message}";
        }

        // App config
        builder.Services.AddSingleton<IMapDefaultsService, MapDefaultsService>();

        // Theme service
        builder.Services.AddSingleton<IThemeService, ThemeService>();

        // Permissions
        builder.Services.AddSingleton<IPermissionsService, PermissionsService>();

        // Connectivity
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

        // ViewModels
        builder.Services.AddTransient<ExploreViewModel>();
        builder.Services.AddTransient<DirectionsViewModel>();
        builder.Services.AddTransient<TrafficViewModel>();
        builder.Services.AddTransient<ToolsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<ExplorePage>();
        builder.Services.AddTransient<DirectionsPage>();
        builder.Services.AddTransient<TrafficPage>();
        builder.Services.AddTransient<ToolsPage>();
        builder.Services.AddTransient<SettingsPage>();

        // Converters
        builder.Services.AddTransient<InverseBoolConverter>();
        builder.Services.AddTransient<BoolToOpenColorConverter>();
        builder.Services.AddTransient<JamFactorToColorConverter>();
        builder.Services.AddTransient<ManeuverActionToIconConverter>();

        return builder.Build();
    }

    private static Stream OpenAppSettingsStream()
    {
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
