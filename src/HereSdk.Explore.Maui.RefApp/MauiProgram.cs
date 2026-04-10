using Here.Explore.Maui;

namespace Here.Explore.Maui.RefApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
               .UseHereSdkExplore(new HereSdkOptions
               {
                   // Replace with your HERE SDK credentials
                   AccessKeyId = "YOUR_ACCESS_KEY_ID",
                   AccessKeySecret = "YOUR_ACCESS_KEY_SECRET"
               });

        return builder.Build();
    }
}