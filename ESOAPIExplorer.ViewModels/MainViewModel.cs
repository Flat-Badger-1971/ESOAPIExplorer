using ESOAPIExplorer.Services;
using ModernWpf.Controls;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private NavigationViewItem _selectedNavigationItem;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        // SelectionChangedCommand = new RelayCommand<NavigationViewSelectionChangedEventArgs>(OnSelectionChanged);
    }

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
                        _ = _navigationService.GoToAsync<HomeViewModel>();
                        break;
                    case nameof(SettingsViewModel):
                        _ = _navigationService.GoToAsync<SettingsViewModel>();
                        break;
                    case nameof(InfoViewModel):
                        _ = _navigationService.GoToAsync<InfoViewModel>();
                        break;
                    case nameof(ExportViewModel):
                        _ = _navigationService.GoToAsync<ExportViewModel>();
                        break;
                }
            }
        }
    }

    //public ICommand SelectionChangedCommand { get; }

    //private void OnSelectionChanged(NavigationViewSelectionChangedEventArgs e)
    //{
    //    if (e.SelectedItem is NavigationViewItem selectedItem)
    //    {
    //        SelectedNavigationItem = selectedItem;
    //    }
    //}

    //public ICommand NavigateCommand => new RelayCommand<string>((string destination) =>
    //{
    //    switch (destination)
    //    {
    //        default:
    //        case nameof(HomeViewModel):
    //            _ = navigationService.GoToAsync<HomeViewModel>();
    //            break;
    //        case nameof(SettingsViewModel):
    //            _ = navigationService.GoToAsync<SettingsViewModel>();
    //            break;
    //        case nameof(InfoViewModel):
    //            _ = navigationService.GoToAsync<InfoViewModel>();
    //            break;
    //        case nameof(ExportViewModel):
    //            _ = navigationService.GoToAsync<ExportViewModel>();
    //            break;
    //    }
    //});
    //public ICommand NavigateHomeCommand => new RelayCommand(() =>
    //{
    //    navigationService.GoToAsync<HomeViewModel>();
    //});
}
