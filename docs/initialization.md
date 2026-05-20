# SDK Initialization

## Setup

In `MauiProgram.cs`, call `UseHereSdkExplore` with your HERE platform credentials:

```csharp
using Here.Explore.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.UseHereSdkExplore(new HereSdkOptions
        {
            AccessKeyId = "YOUR_ACCESS_KEY_ID",
            AccessKeySecret = "YOUR_ACCESS_KEY_SECRET"
        });

        return builder.Build();
    }
}
```

## HereSdkOptions

| Property | Required | Description |
|----------|----------|-------------|
| `AccessKeyId` | Yes | HERE platform access key ID |
| `AccessKeySecret` | Yes | HERE platform access key secret |

## What Happens During Initialization

1. **Platform SDK init**: The Android and iOS HERE SDKs are initialized with your credentials
2. **Service registration**: `IRoutingService`, `ISearchService`, `ITrafficService`, `ILocationService` are registered as DI singletons
3. **Handler registration**: `HereMapViewHandler` is registered for `HereMapView`

## Getting Credentials

1. Go to [developer.here.com](https://developer.here.com/)
2. Create a project or use an existing one
3. Generate an access key pair (Key ID + Secret)
4. Enable the services you need (Map, Search, Routing, Traffic)

The free tier includes 10,000 transactions/month for most services.

## Credential Storage

**Do not hardcode credentials in source code.** Use one of:

```csharp
// Option 1: appsettings.json (embedded resource)
builder.Configuration.AddJsonStream(
    Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("MyApp.appsettings.json")!);

var options = builder.Configuration.GetSection("HereSdk").Get<HereSdkOptions>();

// Option 2: Environment variables
var options = new HereSdkOptions
{
    AccessKeyId = Environment.GetEnvironmentVariable("HERE_ACCESS_KEY_ID")!,
    AccessKeySecret = Environment.GetEnvironmentVariable("HERE_ACCESS_KEY_SECRET")!
};

// Option 3: Secure storage (after first launch)
var options = new HereSdkOptions
{
    AccessKeyId = await SecureStorage.GetAsync("here_key_id"),
    AccessKeySecret = await SecureStorage.GetAsync("here_key_secret")
};
```

## Logging

HERE SDK internal logging uses the platform-native logger:
- **Android**: `android.util.Log` with tag `HERESDK`
- **iOS**: `NSLog` with prefix `HERESDK`

Enable debug logging by setting the log level before initialization:

```csharp
// Android only
Com.Here.Sdk.Core.Engine.SDKLogger.SetLogLevel(
    Com.Here.Sdk.Core.Engine.LogLevel.Debug);
```
