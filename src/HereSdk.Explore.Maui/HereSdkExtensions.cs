using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.Services;
using Microsoft.Maui.Hosting;

namespace Here.Explore.Maui;

/// <summary>
/// Extension methods for registering HERE SDK services with MAUI DI.
/// Services implement IDisposable for native resource cleanup.
/// </summary>
public static class HereSdkExtensions
{
    /// <summary>
    /// Register HERE SDK Explore services with the MAUI app builder.
    /// Call this after creating the MauiAppBuilder.
    /// </summary>
    public static MauiAppBuilder UseHereSdkExplore(this MauiAppBuilder builder, HereSdkOptions options)
    {
        HereSdk.Initialize(options);

        // SearchService, RoutingService, and TrafficService each define a
        // platform-specific `internal void Initialize()` that constructs the
        // native engine. The previous version of this method registered them
        // with `AddSingleton<I, T>()` and never called Initialize, so every
        // operation threw "XxxService not initialized." — the user-visible
        // bug that prompted this fix. Use a factory lambda so Initialize
        // runs once at first resolution.
        //
        // MapService is not registered here: it depends on the MapView and
        // is created and initialized by the HereMapViewHandler.
        // LocationService has no native engine and needs no initialization.
        builder.Services.AddSingleton<IRoutingService>(_ =>
        {
            var s = new RoutingService();
            s.Initialize();
            return s;
        });
        builder.Services.AddSingleton<ISearchService>(_ =>
        {
            var s = new SearchService();
            s.Initialize();
            return s;
        });
        builder.Services.AddSingleton<ITrafficService>(_ =>
        {
            var s = new TrafficService();
            s.Initialize();
            return s;
        });
        builder.Services.AddSingleton<ILocationService, LocationService>();

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<IHereMapView, HereMapViewHandler>();
        });

        return builder;
    }
}