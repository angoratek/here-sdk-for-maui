namespace Here.Explore.Maui;

/// <summary>
/// Provides SDK version and build information.
/// </summary>
public static class SdkInfo
{
    /// <summary>HERE SDK Explore Edition version (sync with Version.props).</summary>
    public const string Version = "4.25.5.0";

    /// <summary>Build configuration this assembly was compiled with.</summary>
#if DEBUG
    public const string BuildConfiguration = "Debug";
#else
    public const string BuildConfiguration = "Release";
#endif

    /// <summary>Current platform identifier.</summary>
    public static string Platform =>
#if ANDROID
        "Android";
#elif IOS
        "iOS";
#else
        "Unknown";
#endif
}
