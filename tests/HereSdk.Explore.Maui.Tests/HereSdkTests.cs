using Xunit;
using Here.Explore.Maui;

namespace Here.Explore.Maui.Tests;

public class HereSdkTests
{
    [Fact]
    public void Initialize_CalledTwice_ThrowsInvalidOperationException()
    {
        // First call succeeds (no-op in unit test context)
        var options = new HereSdkOptions
        {
            AccessKeyId = "test-key",
            AccessKeySecret = "test-secret"
        };

        // Ensure clean state
        HereSdk.Shutdown();

        HereSdk.Initialize(options);

        // Second call should throw
        Assert.Throws<InvalidOperationException>(() => HereSdk.Initialize(options));

        // Clean up
        HereSdk.Shutdown();
    }

    [Fact]
    public void Shutdown_CalledBeforeInit_DoesNotThrow()
    {
        // Shutdown without init should be safe
        HereSdk.Shutdown();
    }

    [Fact]
    public void Shutdown_CalledMultipleTimes_DoesNotThrow()
    {
        var options = new HereSdkOptions
        {
            AccessKeyId = "test-key",
            AccessKeySecret = "test-secret"
        };

        HereSdk.Shutdown();
        HereSdk.Initialize(options);
        HereSdk.Shutdown();
        HereSdk.Shutdown(); // Should not throw
    }

    [Fact]
    public void Initialize_WithCachePath_SetsOptions()
    {
        var options = new HereSdkOptions
        {
            AccessKeyId = "key",
            AccessKeySecret = "secret",
            CachePath = "/tmp/here-cache",
            CachePolicy = HereSdkCachePolicy.OfflineOnly
        };

        // Ensure clean state
        HereSdk.Shutdown();

        // Should not throw (no-op in unit test context)
        HereSdk.Initialize(options);

        // Verify options are set correctly
        Assert.Equal("key", options.AccessKeyId);
        Assert.Equal("secret", options.AccessKeySecret);
        Assert.Equal("/tmp/here-cache", options.CachePath);
        Assert.Equal(HereSdkCachePolicy.OfflineOnly, options.CachePolicy);

        // Clean up
        HereSdk.Shutdown();
    }

    [Fact]
    public void HereSdkOptions_DefaultCachePolicy()
    {
        var options = new HereSdkOptions
        {
            AccessKeyId = "key",
            AccessKeySecret = "secret"
        };

        Assert.Equal(HereSdkCachePolicy.Default, options.CachePolicy);
        Assert.Null(options.CachePath);
    }

    [Fact]
    public void HereSdkCachePolicy_Values()
    {
        Assert.Equal(0, (int)HereSdkCachePolicy.Default);
        Assert.Equal(1, (int)HereSdkCachePolicy.NoCache);
        Assert.Equal(2, (int)HereSdkCachePolicy.OfflineOnly);
    }
}