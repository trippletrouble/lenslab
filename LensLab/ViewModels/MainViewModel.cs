using CommunityToolkit.Mvvm.ComponentModel;

namespace LensLab.ViewModels;

/// <summary>
/// The main ViewModel for the application. Controls general UI state,
/// status message logging, and orchestrates actions across views.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isBusy = false;
}
