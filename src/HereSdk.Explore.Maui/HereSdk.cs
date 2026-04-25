namespace Here.Explore.Maui;

/// <summary>
/// Entry point for HERE SDK Explore Edition initialization.
/// Call <see cref="Initialize"/> once at app startup before using any other API.
/// </summary>
public static class HereSdk
{
    private static bool _initialized;

    /// <summary>Gets whether the HERE SDK has been initialized.</summary>
    public static bool IsInitialized => _initialized;

    /// <summary>
    /// Initialize the HERE SDK for Explore edition.
    /// </summary>
    /// <param name="options">SDK initialization options including credentials.</param>
    /// <exception cref="InvalidOperationException">Thrown if already initialized.</exception>
    public static void Initialize(HereSdkOptions options)
    {
        if (_initialized)
            throw new InvalidOperationException("HERE SDK has already been initialized.");

        InternalInitialize(options);
        _initialized = true;
    }

#if ANDROID
    private static void InternalInitialize(HereSdkOptions options)
    {
        var authMode = Here.Explore.Core.Engine.AuthenticationMode.WithKeySecret(options.AccessKeyId, options.AccessKeySecret);
        var androidOptions = new Here.Explore.Core.Engine.SDKOptions(authMode);
        if (options.CachePath is not null)
            androidOptions.CachePath = options.CachePath;
        Here.Explore.Core.Engine.SDKNativeEngine.MakeSharedInstance(Platform.AppContext, androidOptions);
        // Verify initialization succeeded
        var engine = Here.Explore.Core.Engine.SDKNativeEngine.SharedInstance;
        if (engine is null)
            throw new InvalidOperationException("HERE SDK SDKNativeEngine.SharedInstance is null after MakeSharedInstance. This usually indicates invalid credentials.");
        Android.Util.Log.Info("HereSdk", $"SDK initialized successfully. Engine={engine.GetType().Name}");
    }
#elif IOS
    private static void InternalInitialize(HereSdkOptions options)
    {
        var iosOptions = new Here.Explore.iOS.HereSdkOptions(options.AccessKeyId, options.AccessKeySecret, options.CachePath);
        Here.Explore.iOS.HereSdkEngine.Initialize(iosOptions);
    }
#else
    private static void InternalInitialize(HereSdkOptions options)
    {
        // No-op: platform SDK not available in unit-test context
    }
#endif

    /// <summary>
    /// Shut down the HERE SDK and release resources.
    /// </summary>
    public static void Shutdown()
    {
        if (!_initialized) return;

#if ANDROID
        Here.Explore.Core.Engine.SDKNativeEngine.SharedInstance?.Dispose();
#elif IOS
        Here.Explore.iOS.HereSdkEngine.Shutdown();
#endif
        _initialized = false;
    }
}

/// <summary>
/// Options for initializing the HERE SDK.
/// </summary>
public record HereSdkOptions
{
    /// <summary>HERE SDK access key ID (from your HERE developer account).</summary>
    public required string AccessKeyId { get; init; }

    /// <summary>HERE SDK access key secret (from your HERE developer account).</summary>
    public required string AccessKeySecret { get; init; }

    /// <summary>Optional path for the SDK cache directory. Null uses the default location.</summary>
    public string? CachePath { get; init; }

    /// <summary>Cache policy controlling how map data is cached locally.</summary>
    public HereSdkCachePolicy CachePolicy { get; init; } = HereSdkCachePolicy.Default;
}

/// <summary>
/// Cache policy for the HERE SDK.
/// </summary>
public enum HereSdkCachePolicy
{
    /// <summary>Default caching — cache map data for offline use.</summary>
    Default,

    /// <summary>Disable caching — always fetch from network.</summary>
    NoCache,

    /// <summary>Offline only — use cached data without network access.</summary>
    OfflineOnly,
}