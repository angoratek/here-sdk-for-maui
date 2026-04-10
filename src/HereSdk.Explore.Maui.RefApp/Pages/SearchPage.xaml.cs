using Here.Explore.Maui.RefApp.ViewModels;

namespace Here.Explore.Maui.RefApp;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
        BindingContext = new SearchViewModel();
    }

    public SearchPage(ViewModels.SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}