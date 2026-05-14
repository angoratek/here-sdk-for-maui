using CommunityToolkit.Mvvm.ComponentModel;

namespace Here.Explore.Maui.RefApp.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;
}
