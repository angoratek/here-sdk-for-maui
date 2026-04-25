using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp.Pages;

public partial class ModernMainPage : ContentPage
{
    private readonly ModernMainViewModel _viewModel;

    public ModernMainPage(ModernMainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
