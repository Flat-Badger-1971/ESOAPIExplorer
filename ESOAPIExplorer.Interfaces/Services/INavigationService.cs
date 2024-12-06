using ESOAPIExplorer.EventArguments;
using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services
{
    public interface INavigationService
    {
        Task InitializeAsync();

        Task NavigateToAsync<TViewModel>() where TViewModel : class;

        Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : class;

        Task GoToAsync<TViewModel>() where TViewModel : class;

        Task GoToAsync<TViewModel>(object parameter) where TViewModel : class;

        Task NavigateToAsync(Type viewModelType);

        Task ClearBackStack();

        Task NavigateToAsync(Type viewModelType, object parameter);

        Task NavigateBackAsync();

        Task RemoveLastFromBackStackAsync();

        Task PopToRootAsync();

        event EventHandler<NavigationEventArgs> NavigationPerformed;
    }
}
