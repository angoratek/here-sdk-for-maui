using Microsoft.Extensions.Configuration;
using Xunit;

namespace Here.Explore.Maui.Tests;

public class ConfigLoadingTests
{
    /// <summary>
    /// The committed appsettings.json ships with placeholder credentials so
    /// a fresh clone builds without exposing real secrets. Real credentials
    /// are expected to come from a gitignored appsettings.Local.json at
    /// runtime, loaded as an override by MauiProgram.
    /// </summary>
    [Fact]
    public void AppSettings_Default_HasPlaceholderCredentials()
    {
        // Arrange: locate the committed appsettings.json
        var appsettingsPath = FindAppSettings();

        // Act: load the same way MauiProgram does
        using var stream = File.OpenRead(appsettingsPath);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var keyId = config["HereSdk:AccessKeyId"];
        var keySecret = config["HereSdk:AccessKeySecret"];

        // Assert: keys are present
        Assert.False(string.IsNullOrWhiteSpace(keyId), "HereSdk:AccessKeyId is missing in appsettings.json");
        Assert.False(string.IsNullOrWhiteSpace(keySecret), "HereSdk:AccessKeySecret is missing in appsettings.json");

        // Assert: keys are placeholder values, not real secrets
        Assert.Equal("YOUR_ACCESS_KEY_ID", keyId);
        Assert.Equal("YOUR_ACCESS_KEY_SECRET", keySecret);
    }

    [Fact]
    public void AppSettings_Development_HasPlaceholderCredentials()
    {
        // The Development variant should also contain placeholders
        var appsettingsDevPath = FindAppSettings("appsettings.Development.json");

        using var stream = File.OpenRead(appsettingsDevPath);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var keyId = config["HereSdk:AccessKeyId"];

        // Development file should have placeholder values
        Assert.Equal("YOUR_ACCESS_KEY_ID", keyId);
    }

    [Fact]
    public void HereSdkOptions_CreatedFromConfig_HasValidShape()
    {
        // Simulates MauiProgram's flow: read config → create HereSdkOptions.
        // The committed appsettings.json has placeholders; MauiProgram.cs also
        // reads appsettings.Local.json from AppDataDirectory as an override,
        // but this test runs in-process against the committed file only.
        var appsettingsPath = FindAppSettings();

        using var stream = File.OpenRead(appsettingsPath);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var keyId = config["HereSdk:AccessKeyId"];
        var keySecret = config["HereSdk:AccessKeySecret"];

        Assert.False(string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret),
            "HereSdk:AccessKeyId or AccessKeySecret is missing/empty in appsettings.json");

        var options = new HereSdkOptions
        {
            AccessKeyId = keyId!,
            AccessKeySecret = keySecret!
        };

        Assert.Equal(keyId, options.AccessKeyId);
        Assert.Equal(keySecret, options.AccessKeySecret);
    }

    private static string FindAppSettings(string filename = "appsettings.json")
    {
        var baseDir = AppContext.BaseDirectory;
        var dir = new DirectoryInfo(baseDir);
        for (int i = 0; i < 8; i++)
        {
            // Check current dir
            var candidate = Path.Combine(dir.FullName, filename);
            if (File.Exists(candidate))
                return candidate;

            // Check RefApp subdirectory
            var refAppCandidate = Path.Combine(dir.FullName, "src", "HereSdk.Explore.Maui.RefApp", filename);
            if (File.Exists(refAppCandidate))
                return refAppCandidate;

            dir = dir.Parent!;
            if (dir is null) break;
        }

        // Try repo root
        var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        var repoAppSettings = Path.Combine(repoRoot, "src", "HereSdk.Explore.Maui.RefApp", filename);
        if (File.Exists(repoAppSettings))
            return repoAppSettings;

        throw new FileNotFoundException($"Could not find {filename}. Searched from {baseDir} up to {repoRoot}");
    }
}