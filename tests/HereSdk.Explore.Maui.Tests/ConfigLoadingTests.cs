using Microsoft.Extensions.Configuration;
using Xunit;

namespace Here.Explore.Maui.Tests;

public class ConfigLoadingTests
{
    /// <summary>
    /// Tests that appsettings.json can be loaded and parsed the same way
    /// MauiProgram does it (via AddJsonStream), and that credentials are present.
    /// </summary>
    [Fact]
    public void AppSettings_HasNonPlaceholderCredentials()
    {
        // Arrange: locate appsettings.json relative to test assembly
        var appsettingsPath = FindAppSettings();

        // Act: load the same way MauiProgram does
        using var stream = File.OpenRead(appsettingsPath);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var keyId = config["HereSdk:AccessKeyId"];
        var keySecret = config["HereSdk:AccessKeySecret"];

        // Assert: credentials exist
        Assert.False(string.IsNullOrWhiteSpace(keyId), "HereSdk:AccessKeyId is missing or empty in appsettings.json");
        Assert.False(string.IsNullOrWhiteSpace(keySecret), "HereSdk:AccessKeySecret is missing or empty in appsettings.json");

        // Assert: credentials are not placeholder values
        Assert.NotEqual("YOUR_ACCESS_KEY_ID", keyId);
        Assert.NotEqual("YOUR_ACCESS_KEY_SECRET", keySecret);

        // Assert: key format looks valid (HERE keys are typically base64-ish, 20+ chars)
        Assert.True(keyId.Length >= 10, $"AccessKeyId seems too short ({keyId.Length} chars). Expected 10+ chars.");
        Assert.True(keySecret.Length >= 20, $"AccessKeySecret seems too short ({keySecret.Length} chars). Expected 20+ chars.");
    }

    [Fact]
    public void AppSettings_Development_HasPlaceholderCredentials()
    {
        // The Development variant should contain placeholders
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
    public void HereSdkOptions_CreatedFromConfig_HasValidCredentials()
    {
        // Simulates MauiProgram's exact flow: read config → create HereSdkOptions
        var appsettingsPath = FindAppSettings();

        using var stream = File.OpenRead(appsettingsPath);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var keyId = config["HereSdk:AccessKeyId"];
        var keySecret = config["HereSdk:AccessKeySecret"];

        // This is the same code path as MauiProgram.CreateMauiApp()
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