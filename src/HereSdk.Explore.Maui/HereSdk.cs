namespace Here.Explore.Maui;

/// <summary>
/// Entry point for HERE SDK Explore Edition initialization.
/// Call <see cref="Initialize"/> once at app startup before using any other API.
/// </summary>
public static class HereSdk
{
    private static bool _initialized;

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
        var androidOptions = new Com.Here.Sdk.Core.Engine.SDKOptions
        {
            AccessKeyId = options.AccessKeyId,
            AccessKeySecret = options.AccessKeySecret,
        };
        Com.Here.Sdk.Core.Engine.SDKNativeEngine.CreateInstance(androidOptions);
    }
#elif IOS
    private static void InternalInitialize(HereSdkOptions options)
    {
        var iosOptions = new HereSdkOptions(options.AccessKeyId, options.AccessKeySecret, options.CachePath);
        HereSdkEngine.Initialize(iosOptions);
    }
#endif

    /// <summary>
    /// Shut down the HERE SDK and release resources.
    /// </summary>
    public static void Shutdown()
    {
        if (!_initialized) return;

#if ANDROID
        Com.Here.Sdk.Core.Engine.SDKNativeEngine.Instance?.Dispose();
#elif IOS
        HereSdkEngine.Shutdown();
#endif
        _initialized = false;
    }
}

/// <summary>
/// Options for initializing the HERE SDK.
/// </summary>
public record HereSdkOptions
{
    public required string AccessKeyId { get; init; }
    public required string AccessKeySecret { get; init; }
    public string? CachePath { get; init; }
    public HereSdkCachePolicy CachePolicy { get; init; } = HereSdkCachePolicy.Default;
}

/// <summary>
/// Cache policy for the HERE SDK.
/// </summary>
public enum HereSdkCachePolicy
{
    Default,
    NoCache,
    OfflineOnly,
}