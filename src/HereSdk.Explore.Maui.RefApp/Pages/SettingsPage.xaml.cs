using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
