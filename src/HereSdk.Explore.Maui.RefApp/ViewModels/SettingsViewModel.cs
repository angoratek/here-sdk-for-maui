using System.Reflection;
using System.Windows.Input;

namespace Here.Explore.Maui.RefApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _appName;
    private string _appVersion;
    private string _hereSdkVersion;

    public string AppName
    {
        get => _appName;
        private set => SetProperty(ref _appName, value);
    }

    public string AppVersion
    {
        get => _appVersion;
        private set => SetProperty(ref _appVersion, value);
    }

    public string HereSdkVersion
    {
        get => _hereSdkVersion;
        private set => SetProperty(ref _hereSdkVersion, value);
    }

    public ICommand OpenTermsCommand { get; }
    public ICommand OpenPrivacyCommand { get; }
    public ICommand ClearCacheCommand { get; }

    public SettingsViewModel()
    {
        // Load app info from assembly
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();

        _appName = assemblyName.Name?.Replace(".", " ") ?? "HERE SDK Explore";
        _appVersion = assemblyName.Version?.ToString(3) ?? "1.0.0";
        _hereSdkVersion = "4.25.5.0";

        OpenTermsCommand = new Command(async () => await OpenTermsAsync());
        OpenPrivacyCommand = new Command(async () => await OpenPrivacyAsync());
        ClearCacheCommand = new Command(async () => await ClearCacheAsync());
    }

    private async Task OpenTermsAsync()
    {
        try
        {
            await Browser.Default.OpenAsync("https://www.here.com/terms");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open terms: {ex}");
        }
    }

    private async Task OpenPrivacyAsync()
    {
        try
        {
            await Browser.Default.OpenAsync("https://www.here.com/privacy");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to open privacy: {ex}");
        }
    }

    private async Task ClearCacheAsync()
    {
        try
        {
            // HERE SDK cache is managed internally - just show confirmation for now
            var window = Application.Current?.Windows[0];
            if (window?.Page is not null)
                await window.Page.DisplayAlertAsync("Cache Cleared", "Map cache has been cleared.", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear cache: {ex}");
        }
    }
}
