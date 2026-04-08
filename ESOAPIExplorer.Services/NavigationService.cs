using ESOAPIExplorer.EventArguments;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _Container;
    private readonly IEventService _EventService;
    private readonly IDialogService _DialogService;
    protected MainWindow CurrentApplication;
    protected Frame MainFrame;

    public NavigationService(IServiceProvider container, MainWindow mainWindow, IEventService eventService, IDialogService dialogService)
    {
        _Container = container;
        CurrentApplication = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _EventService = eventService;
        _DialogService = dialogService;
        MainFrame = CurrentApplication.NavigationFrame ?? throw new InvalidOperationException("The main window navigation frame has not been initialized.");
        MainFrame.NavigationFailed += CurrentApplication_NavigationFailed;
    }

    public async Task InitializeAsync()
    {
        CurrentApplication.MainContainer.DataContext = _Container.GetRequiredService<MainViewModel>();
        await GoToAsync<HomeViewModel>();
    }

    public Task ClearBackStack()
    {
        if (MainFrame.BackStack.Count > 0)
        {
            MainFrame.BackStack.Clear();
        }

        return Task.CompletedTask;
    }

    public async Task NavigateBackAsync()
    {
        await Task.Delay(1);

        if (CurrentApplication != null)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }

    public virtual Task RemoveLastFromBackStackAsync()
    {
        if (CurrentApplication != null && MainFrame.BackStack.Count >= 2)
        {
            MainFrame.BackStack.Remove(MainFrame.BackStack[MainFrame.BackStack.Count - 2]);
        }

        return Task.FromResult(true);
    }

    public Task PopToRootAsync()
    {
        while (MainFrame.CanGoBack)
        {
            MainFrame.GoBack();
        }

        return Task.CompletedTask;
    }

    public async Task GoToAsync<TViewModel>() where TViewModel : class
    {
        ValidateViewModelType(typeof(TViewModel));

        await InternalNavigateToAsync(typeof(TViewModel), null);
        await ClearBackStack();
    }

    public async Task GoToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        ValidateViewModelType(typeof(TViewModel));

        await InternalNavigateToAsync(typeof(TViewModel), parameter);
        await ClearBackStack();
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : class
    {
        ValidateViewModelType(typeof(TViewModel));

        return InternalNavigateToAsync(typeof(TViewModel), null);
    }

    public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        ValidateViewModelType(typeof(TViewModel));

        return InternalNavigateToAsync(typeof(TViewModel), parameter);
    }

    public Task NavigateToAsync(Type viewModelType)
    {
        ValidateViewModelType(viewModelType);
        return InternalNavigateToAsync(viewModelType, null);
    }

    public Task NavigateToAsync(Type viewModelType, object parameter)
    {
        ValidateViewModelType(viewModelType);
        return InternalNavigateToAsync(viewModelType, parameter);
    }

    protected virtual async Task InternalNavigateToAsync(Type viewModelType, object parameter)
    {
        object vm = _Container.GetRequiredService(viewModelType);
        Type pageType = GetPageTypeForViewModel(viewModelType);

        bool navigated = MainFrame.Navigate(pageType);

        if (!navigated || MainFrame.Content is not Page page)
        {
            throw new InvalidOperationException($"Navigation to page '{pageType.FullName}' for view model '{viewModelType.FullName}' failed.");
        }

        page.DataContext = vm;
        OnNavigationPerformed(viewModelType);

        await ((ViewModelBase)vm).InitializeAsync(parameter);
    }

    private void CurrentApplication_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
    {
        string sourcePageType = e.SourcePageType?.FullName ?? "<unknown>";
        throw new InvalidOperationException($"Navigation to page '{sourcePageType}' failed. Ensure the page exists, is public, and can be constructed by the frame.", e.Exception);
    }

    protected static Type GetPageTypeForViewModel(Type viewModelType)
    {
        ValidateViewModelType(viewModelType);

        string ns = typeof(MainWindow).Namespace;
        string typeFullName = $"{ns}.{viewModelType.Name.Replace("ViewModel", "View")}";
        Type type = GetType(typeFullName);

        if (type == null)
        {
            throw new InvalidOperationException($"No view could be found for view model '{viewModelType.FullName}'. Expected view type '{typeFullName}'.");
        }

        if (!typeof(Page).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"Mapped view type '{type.FullName}' for view model '{viewModelType.FullName}' is not a Page.");
        }

        return type;
    }

    private static void ValidateViewModelType(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        if (!typeof(ViewModelBase).IsAssignableFrom(viewModelType))
        {
            throw new InvalidOperationException($"Type '{viewModelType.FullName}' is not derived from '{typeof(ViewModelBase).FullName}'.");
        }

        if (!viewModelType.Name.EndsWith("ViewModel", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Invalid view model type '{viewModelType.FullName}'. Expected the type name to end with 'ViewModel'.");
        }
    }

    public static Type GetType(string typeName)
    {
        Type type = Type.GetType(typeName);

        if (type != null)
        {
            return type;
        }

        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);

            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    protected virtual void OnNavigationPerformed(Type viewModelType)
    {
        NavigationEventArgs args = new() { ViewModelType = viewModelType };
        NavigationPerformed?.Invoke(this, args);
    }

    public event EventHandler<NavigationEventArgs> NavigationPerformed;
}
