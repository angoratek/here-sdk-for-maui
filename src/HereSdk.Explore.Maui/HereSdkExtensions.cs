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

        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<ISearchService, SearchService>();
        builder.Services.AddSingleton<ITrafficService, TrafficService>();

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<IHereMapView, HereMapViewHandler>();
        });

        return builder;
    }
}