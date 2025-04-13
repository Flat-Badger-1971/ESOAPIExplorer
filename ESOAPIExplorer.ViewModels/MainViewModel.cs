using ESOAPIExplorer.Services;
using ModernWpf.Controls;

namespace ESOAPIExplorer.ViewModels;

public class MainViewModel(INavigationService navigationService) : ViewModelBase
{
    private NavigationViewItem _selectedNavigationItem;

    public NavigationViewItem SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            SetProperty(ref _selectedNavigationItem, value);

            if (value?.Tag != null)
            {
                switch (value.Tag.ToString())
                {
                    default:
                    case nameof(HomeViewModel):
                        _ = navigationService.GoToAsync<HomeViewModel>();
                        break;
                    case nameof(SettingsViewModel):
                        _ = navigationService.GoToAsync<SettingsViewModel>();
                        break;
                    case nameof(InfoViewModel):
                        _ = navigationService.GoToAsync<InfoViewModel>();
                        break;
                    case nameof(ExportViewModel):
                        _ = navigationService.GoToAsync<ExportViewModel>();
                        break;
                }
            }
        }
    }
}
