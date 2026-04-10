namespace Here.Explore.Maui.Models;

/// <summary>
/// Error codes for SDK instantiation failures.
/// </summary>
public enum InstantiationErrorCode
{
    None,
    NetworkError,
    InvalidCredentials,
    InvalidOptions,
    AlreadyInitialized,
    EngineDisposed,
    InternalError
}

/// <summary>
/// SDK log level.
/// </summary>
public enum LogLevel
{
    Emergency,
    Alert,
    Critical,
    Error,
    Warning,
    Notice,
    Info,
    Debug
}

/// <summary>
/// Unit system for measurements.
/// </summary>
public enum UnitSystem
{
    Metric,
    ImperialUk,
    ImperialUs
}

/// <summary>
/// Cache policy for the SDK.
/// </summary>
public enum CachePolicy
{
    Default,
    NoCache,
    OfflineOnly
}

/// <summary>
/// Engine base URL options.
/// </summary>
public enum EngineBaseUrl
{
    DefaultUrl,
    ChinaUrl
}