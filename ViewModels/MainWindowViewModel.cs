using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicLibrary.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel = null!;
}