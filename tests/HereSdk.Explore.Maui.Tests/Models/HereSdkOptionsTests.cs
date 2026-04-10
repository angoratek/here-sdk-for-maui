using Here.Explore.Maui.Models;

namespace Here.Explore.Maui.Tests.Models;

public class HereSdkOptionsTests
{
    [Fact]
    public void Constructor_SetsRequiredFields()
    {
        var options = new HereSdkOptions
        {
            AccessKeyId = "test-key",
            AccessKeySecret = "test-secret"
        };
        Assert.Equal("test-key", options.AccessKeyId);
        Assert.Equal("test-secret", options.AccessKeySecret);
        Assert.Null(options.CachePath);
        Assert.Equal(CachePolicy.Default, options.CachePolicy);
    }

    [Fact]
    public void Constructor_WithOptionalFields_SetsAll()
    {
        var options = new HereSdkOptions
        {
            AccessKeyId = "key",
            AccessKeySecret = "secret",
            CachePath = "/tmp/here-cache",
            CachePolicy = CachePolicy.OfflineOnly
        };
        Assert.Equal("/tmp/here-cache", options.CachePath);
        Assert.Equal(CachePolicy.OfflineOnly, options.CachePolicy);
    }
}