namespace Here.Explore.Maui.Services;

/// <summary>
/// Base interface for HERE SDK services. All services that wrap
/// native SDK engines should implement IDisposable to release
/// native resources.
/// </summary>
public interface IHereSdkService : IDisposable;