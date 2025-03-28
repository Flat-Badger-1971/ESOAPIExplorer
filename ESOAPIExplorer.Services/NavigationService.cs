using ESOAPIExplorer.Models;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using System;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ESOAPIExplorer.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _Container;
    private readonly IEventService _EventService;
    private readonly IDialogService _DialogService;
    protected Views.MainWindow CurrentApplication;
    protected Frame MainFrame;

    public NavigationService(IServiceProvider container, IEventService eventService, IDialogService dialogService)
    {
        _Container = container;
        _EventService = eventService;
        _DialogService = dialogService;

        CurrentApplication = (Views.MainWindow)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
        MainFrame = (CurrentApplication).NavigationFrame;

        MainFrame.Navigated += async (sender, e) =>
        {
            NavigationModel nav = (NavigationModel)e.ExtraData;
            Type viewModelType = nav?.ViewModel?.GetType();
            MainFrame.DataContext = nav?.ViewModel;
            OnNavigationPerformed(viewModelType);
            await(nav.ViewModel as ViewModelBase).InitializeAsync(nav.Parameter);
        };

    }
    

    public async Task InitializeAsync()
    {
        CurrentApplication.MainContainer.DataContext = _Container.GetService(typeof(MainViewModel));
        await GoToAsync<HomeViewModel>();
    }

    public Task ClearBackStack()
    {
        try
        {
            //MainFrame.BackStack.Clear();
            return Task.CompletedTask;
        }
        finally { }
    }

    public async Task NavigateBackAsync()
    {
        await Task.Delay(1);

        if (CurrentApplication is not null and MainWindow)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
        else if (CurrentApplication.Content != null)
        {
            CurrentApplication.Content = new Page();
        }
    }

    public virtual Task RemoveLastFromBackStackAsync()
    {
        if (CurrentApplication is not null and MainWindow)
        {
            MainFrame.RemoveBackEntry();// .BackStack.Remove(MainFrame.BackStack[MainFrame.BackStack.Count() - 2]);
        }

        return Task.FromResult(true);
    }

    public async Task PopToRootAsync()
    {
        await Task.Delay(1);
        throw new NotImplementedException();
    }

    public async Task GoToAsync<TViewModel>() where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
        {
            throw new Exception("View model is not derived from a valid type");
        }

        await ClearBackStack();
        await InternalNavigateToAsync(typeof(TViewModel), null);
    }

    public async Task GoToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
        {
            throw new Exception("View model is not derived from a valid type");
        }

        await ClearBackStack();
        await InternalNavigateToAsync(typeof(TViewModel), parameter);
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
        {
            throw new Exception("View model is not derived from a valid type");
        }

        return InternalNavigateToAsync(typeof(TViewModel), null);
    }

    public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
        {
            throw new Exception("View model is not derived from a valid type");
        }

        return InternalNavigateToAsync(typeof(TViewModel), parameter);
    }

    public Task NavigateToAsync(Type viewModelType) => InternalNavigateToAsync(viewModelType, null);
    public Task NavigateToAsync(Type viewModelType, object parameter) => InternalNavigateToAsync(viewModelType, parameter);

    protected virtual async Task InternalNavigateToAsync(Type viewModelType, object parameter)
    {
        object vm = _Container.GetService(viewModelType);
        object nav = new NavigationModel { ViewModel = vm, Parameter = parameter };
        Type pageType = GetPageTypeForViewModel(viewModelType);

        // Do not repeat app initialisation when the Window already has content,
        // just ensure that the window is active
        if (Application.Current.MainWindow.Content == null)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            //MainFrame = new Frame();
            MainFrame.NavigationFailed += CurrentApplication_NavigationFailed;

            // Place the frame in the current Window
            Application.Current.MainWindow.Content = MainFrame;
        }

        //if (vm is MainViewModel)
        //{
        //    MainFrame.Navigate(pageType,nav);
        //   // ((Page)MainFrame.Content).DataContext = vm;
        //}

        MainFrame.Navigate(pageType,nav);
    }

    private void CurrentApplication_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new NotImplementedException();
    }

    protected static Type GetPageTypeForViewModel(Type viewModelType)
    {
        if (!viewModelType.Name.EndsWith("ViewModel"))
        {
            throw new Exception($"Invalid type for ViewModel ({viewModelType.Name})");
        }

        string ns = typeof(MainWindow).Namespace;
        string typeFullName = $"{ns}.{viewModelType.Name.Replace("ViewModel", "View")}";
        Type type = GetType(typeFullName);

        return type;
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

    protected static Page CreatePage(Type viewModelType)
    {
        Type pageType = GetPageTypeForViewModel(viewModelType) ?? throw new Exception($"Mapping type for {viewModelType} is not a page");

        try
        {
            Page page = Activator.CreateInstance(pageType) as Page;

            return page;
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected Page GetPage(Type viewModelType)
    {
        Type pageType = GetPageTypeForViewModel(viewModelType) ?? throw new Exception($"Mapping type for {viewModelType} is not a page");

        try
        {
            Page page = _Container.GetService(pageType) as Page;

            return page;
        }
        catch (Exception)
        {
            return null;
        }
    }

    protected virtual void OnNavigationPerformed(Type viewModelType)
    {
        EventArguments.NavigationEventArgs args = new() { ViewModelType = viewModelType };
        NavigationPerformed?.Invoke(this, args);
    }

    public event EventHandler<EventArguments.NavigationEventArgs> NavigationPerformed;
}
