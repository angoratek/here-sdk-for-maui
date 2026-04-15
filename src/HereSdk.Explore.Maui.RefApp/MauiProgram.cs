using Here.Explore.Maui;
using Microsoft.Extensions.Configuration;

namespace Here.Explore.Maui.RefApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Load configuration from appsettings files
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var options = new HereSdkOptions
        {
            AccessKeyId = config["HereSdk:AccessKeyId"]
                ?? throw new InvalidOperationException("HereSdk:AccessKeyId not found in appsettings.json"),
            AccessKeySecret = config["HereSdk:AccessKeySecret"]
                ?? throw new InvalidOperationException("HereSdk:AccessKeySecret not found in appsettings.json")
        };

        builder.UseMauiApp<App>()
               .UseHereSdkExplore(options);

        return builder.Build();
    }
}