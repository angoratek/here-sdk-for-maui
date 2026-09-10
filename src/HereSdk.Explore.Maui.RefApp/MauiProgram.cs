using Here.Explore.Maui;
using Here.Explore.Maui.Controls;
using Here.Explore.Maui.Handlers;
using Here.Explore.Maui.RefApp.Converters;
using Here.Explore.Maui.RefApp.Pages;
using Here.Explore.Maui.RefApp.Services;
using Here.Explore.Maui.RefApp.ViewModels;
using Here.Explore.Maui.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Hosting;

namespace Here.Explore.Maui.RefApp;

public static class MauiProgram
{
    /// <summary>Initialization error message (null if init succeeded).</summary>
    public static string? InitError { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.ConfigureFonts(fonts =>
        {
            // Note: MauiFont flattens the files to assets/<name>.ttf in the
            // package, so register by bare filename — the "Resources/Fonts/"
            // path form (older MAUI templates) does not resolve here.
            fonts.AddFont("Inter-Regular.ttf", "InterRegular");
            fonts.AddFont("Inter-Medium.ttf", "InterMedium");
            fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            fonts.AddFont("Inter-Bold.ttf", "InterBold");
            fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
        });

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<IHereMapView, HereMapViewHandler>();
        });

        // Load configuration — embedded appsettings.json is the base.
        // Overrides (later wins):
        //   1. appsettings.Local.json bundled as a MauiAsset (dev machines that
        //      ship real HERE SDK credentials via a gitignored local file).
        //   2. appsettings.Local.json in AppDataDirectory (runtime override;
        //      copied to the device's writable storage).
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonStream(OpenAppSettingsStream());

        var localPath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.Local.json");
        TrySeedLocalAppSettings(localPath);
        if (File.Exists(localPath))
        {
            configBuilder.AddJsonFile(localPath, optional: false, reloadOnChange: false);
        }

        var config = configBuilder.Build();
        builder.Configuration.AddConfiguration(config);

        // Initialize HERE SDK — errors are captured in InitError for UI display
        InitError = null;
        string? keyId = null;
        string? keySecret = null;
        try
        {
            keyId = config["HereSdk:AccessKeyId"];
            keySecret = config["HereSdk:AccessKeySecret"];

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

            // SearchService, RoutingService, and TrafficService each have a
            // platform-specific Initialize() that constructs the native
            // engine. The previous AddSingleton<I, T>() registrations
            // skipped that call, so every operation threw
            // "XxxService not initialized." — the user-visible bug this
            // factory pattern fixes. LocationService has no native engine.
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
        }
        catch (Exception ex)
        {
            InitError ??= $"{ex.GetType().Name}: {ex.Message}";
        }

        // Diagnostic: surface init status so the user can see why the map is
        // blank (e.g. invalid credentials, network blocked). Goes to both
        // Console.WriteLine (Android logcat, iOS Console) and a file in
        // AppDataDirectory that survives across launches.
        Console.WriteLine($"[REFAPP_INIT] {(InitError is null ? "OK" : $"FAIL: {InitError}")}");
        try
        {
            var diagPath = Path.Combine(FileSystem.AppDataDirectory, "refapp-init.log");
            File.WriteAllText(diagPath,
                $"[{DateTime.UtcNow:O}] InitError={(InitError ?? "<none>")}\n" +
                $"keyIdLen={keyId?.Length ?? 0}\n" +
                $"keySecretLen={keySecret?.Length ?? 0}\n" +
                $"configKeys={string.Join(",", config.AsEnumerable().Select(kv => kv.Key))}\n");
        }
        catch
        {
            // best-effort diagnostic; never fail app startup over a log write
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
        builder.Services.AddTransient<TrafficIncidentTypeToIconConverter>();

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

    private static void TrySeedLocalAppSettings(string targetPath)
    {
        // If a developer added appsettings.Local.json as a MauiAsset (the
        // csproj does this only when the file exists on the build machine),
        // copy it to the device's AppDataDirectory on first run. Subsequent
        // runs find the file already in place and skip the copy. The runtime
        // file always wins over any later bundle changes until the user
        // deletes it.
        if (File.Exists(targetPath))
        {
            return;
        }

        try
        {
            using var bundled = FileSystem.OpenAppPackageFileAsync("appsettings.Local.json")
                .GetAwaiter()
                .GetResult();
            using var output = File.Create(targetPath);
            bundled.CopyTo(output);
        }
        catch (FileNotFoundException)
        {
            // No bundled override — leave targetPath non-existent so the
            // next config source is skipped.
        }
        catch
        {
            // Other failures (e.g. missing AssetManager entry) are non-fatal;
            // we still want the app to start with the base appsettings.json.
        }
    }
}
