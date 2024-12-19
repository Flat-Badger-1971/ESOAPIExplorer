// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public class MainViewModel(INavigationService navigationService) :ViewModelBase
{
    private NavigationViewItem _SelectedNavigationItem;
    public NavigationViewItem SelectedNavigationItem 
    { 
        get => _SelectedNavigationItem;
        set
        {
            SetProperty(ref _SelectedNavigationItem, value);
            switch (value.Tag.ToString())
            {
                default:
                case nameof(HomeViewModel):
                    _ = navigationService.GoToAsync<HomeViewModel>();
                    break;
                case nameof(SettingsViewModel):
                    _ = navigationService.GoToAsync<SettingsViewModel>();
                    break;
            }
        }
    }
}
