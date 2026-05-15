using Microsoft.Maui.ApplicationModel;

namespace Here.Explore.Maui.RefApp.Services;

public interface IPermissionsService
{
    bool IsLocationGranted { get; }
    event EventHandler<bool>? LocationPermissionChanged;
    Task<bool> RequestLocationPermissionAsync();
}

public class PermissionsService : IPermissionsService
{
    public bool IsLocationGranted { get; private set; }

    public event EventHandler<bool>? LocationPermissionChanged;

    public PermissionsService()
    {
        IsLocationGranted = Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>()
            .GetAwaiter().GetResult() == PermissionStatus.Granted;
    }

    public async Task<bool> RequestLocationPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
        {
            IsLocationGranted = true;
            return true;
        }

        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page is not null)
            {
                var accepted = await page.DisplayAlertAsync(
                    "Location Permission",
                    "HERE SDK Explore needs your location to show you on the map and search for nearby places.",
                    "Allow",
                    "Not Now");

                if (!accepted)
                {
                    IsLocationGranted = false;
                    LocationPermissionChanged?.Invoke(this, false);
                    return false;
                }
            }
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        IsLocationGranted = status == PermissionStatus.Granted;
        LocationPermissionChanged?.Invoke(this, IsLocationGranted);
        return IsLocationGranted;
    }
}
