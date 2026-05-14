namespace Here.Explore.Maui.RefApp.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    void SetDarkMode(bool enabled);
}

public class ThemeService : IThemeService
{
    public bool IsDarkMode => Application.Current?.UserAppTheme == AppTheme.Dark;

    public void SetDarkMode(bool enabled)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = enabled ? AppTheme.Dark : AppTheme.Light;
    }
}
