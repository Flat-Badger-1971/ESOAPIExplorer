using ESOAPIExplorer.EventArguments;
using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface INavigationService
{
    public Task InitializeAsync();
    public Task NavigateToAsync<TViewModel>() where TViewModel : class;
    public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : class;
    public Task GoToAsync<TViewModel>() where TViewModel : class;
    public Task GoToAsync<TViewModel>(object parameter) where TViewModel : class;
    public Task NavigateToAsync(Type viewModelType);
    public Task ClearBackStack();
    public Task NavigateToAsync(Type viewModelType, object parameter);
    public Task NavigateBackAsync();
    public Task RemoveLastFromBackStackAsync();
    public Task PopToRootAsync();
    public event EventHandler<NavigationEventArgs> NavigationPerformed;
}
