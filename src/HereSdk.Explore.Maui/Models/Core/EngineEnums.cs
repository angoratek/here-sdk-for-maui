namespace Here.Explore.Maui.Models;

/// <summary>
/// Error codes for SDK instantiation failures.
/// </summary>
public enum InstantiationErrorCode
{
    /// <summary>No error — initialization succeeded.</summary>
    None,
    /// <summary>Network error during initialization.</summary>
    NetworkError,
    /// <summary>Invalid credentials (access key ID or secret).</summary>
    InvalidCredentials,
    /// <summary>Invalid SDK options provided.</summary>
    InvalidOptions,
    /// <summary>SDK has already been initialized.</summary>
    AlreadyInitialized,
    /// <summary>SDK engine has been disposed.</summary>
    EngineDisposed,
    /// <summary>Internal error during initialization.</summary>
    InternalError
}

/// <summary>
/// SDK log level.
/// </summary>
public enum LogLevel
{
    /// <summary>System is unusable.</summary>
    Emergency,
    /// <summary>Immediate action required.</summary>
    Alert,
    /// <summary>Critical conditions.</summary>
    Critical,
    /// <summary>Error conditions.</summary>
    Error,
    /// <summary>Warning conditions.</summary>
    Warning,
    /// <summary>Normal but significant conditions.</summary>
    Notice,
    /// <summary>Informational messages.</summary>
    Info,
    /// <summary>Detailed debug messages.</summary>
    Debug
}

/// <summary>
/// Unit system for measurements.
/// </summary>
public enum UnitSystem
{
    /// <summary>Metric system (kilometers, meters).</summary>
    Metric,
    /// <summary>Imperial UK (miles, yards).</summary>
    ImperialUk,
    /// <summary>Imperial US (miles, feet).</summary>
    ImperialUs
}

/// <summary>
/// Cache policy for the SDK.
/// </summary>
public enum CachePolicy
{
    /// <summary>Default caching behavior.</summary>
    Default,
    /// <summary>Disable caching.</summary>
    NoCache,
    /// <summary>Use cached data only (no network).</summary>
    OfflineOnly
}

/// <summary>
/// Engine base URL options.
/// </summary>
public enum EngineBaseUrl
{
    /// <summary>Default HERE endpoint.</summary>
    DefaultUrl,
    /// <summary>China-specific endpoint.</summary>
    ChinaUrl
}