using ESOAPIExplorer.EventArguments;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _Container;
    private readonly IEventService _EventService;
    private readonly IDialogService _DialogService;
    protected MainWindow CurrentApplication;
    protected Frame MainFrame;

    public NavigationService(IServiceProvider container, IEventService eventService, IDialogService dialogService)
    {

        _Container = container;

        CurrentApplication = (MainWindow)Application.Current.GetType().GetProperty("Window").GetValue(Application.Current);
        MainFrame = CurrentApplication.NavigationFrame;

        _EventService = eventService;
        _DialogService = dialogService;
    }

    public async Task InitializeAsync()
    {
        CurrentApplication.MainContainer.DataContext = _Container.GetRequiredService<MainViewModel>();
        await GoToAsync<HomeViewModel>();
    }

    public Task ClearBackStack()
    {
        try
        {
            MainFrame.BackStack.Clear();
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
                MainFrame.GoBack();
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
            MainFrame.BackStack.Remove( //was NavFrame
                MainFrame.BackStack[MainFrame.BackStack.Count - 2]);
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
            throw new Exception("View model is not derived from a valid type");
        await ClearBackStack();
        await InternalNavigateToAsync(typeof(TViewModel), null);
    }
    public async Task GoToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
            throw new Exception("View model is not derived from a valid type");
        await ClearBackStack();
        await InternalNavigateToAsync(typeof(TViewModel), parameter);
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
            throw new Exception("View model is not derived from a valid type");
        return InternalNavigateToAsync(typeof(TViewModel), null);
    }

    public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : class
    {
        if (typeof(TViewModel).BaseType != typeof(ViewModelBase))
            throw new Exception("View model is not derived from a valid type");
        return InternalNavigateToAsync(typeof(TViewModel), parameter);
    }

    public Task NavigateToAsync(Type viewModelType)
    {
        return InternalNavigateToAsync(viewModelType, null);
    }

    public Task NavigateToAsync(Type viewModelType, object parameter)
    {
        return InternalNavigateToAsync(viewModelType, parameter);
    }


    protected virtual async Task InternalNavigateToAsync(Type viewModelType, object parameter)
    {
        object vm = _Container.GetRequiredService(viewModelType);
        Type pageType = GetPageTypeForViewModel(viewModelType);

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (CurrentApplication.Content == null)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            var frame = new Frame();
            frame.NavigationFailed += CurrentApplication_NavigationFailed;

            // Place the frame in the current Window
            Window.Current.Content = frame;
        }


        if (vm is MainViewModel)
        {
            //MainFrame.Content = page;
            MainFrame.Navigate(pageType);
            ((Page)MainFrame.Content).DataContext = vm;
        }
        //else if (((Frame)CurrentApplication.Content).Content is MainWindow mainPage)
        //{
            MainFrame.Navigate(pageType);
            ((Page)MainFrame.Content).DataContext = vm;
            OnNavigationPerformed(viewModelType);
        //}
        //else if (!(MainFrame.Content is MainWindow))
        //{
        //    if (_MainVm == null)
        //    {
        //        _MainVm = _Container.GetRequiredService<MainViewModel>();
        //    }
        //    MainFrame.Navigate(typeof(MainWindow));
        //    ((Page)MainFrame.Content).DataContext = _MainVm;
        //    await (_MainVm as ViewModelBase).InitializeAsync(parameter);
        //    await NavigateToAsync(pageType, parameter);
        //    return;
        //}

        await (vm as ViewModelBase).InitializeAsync(parameter);
    }

    private void CurrentApplication_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
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
        var type = Type.GetType(typeName);
        if (type != null) return type;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null)
                return type;
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
            Page page = _Container.GetRequiredService(pageType) as Page;
            return page;
        }
        catch (Exception)
        {
            return null;
        }
    }
    protected virtual void OnNavigationPerformed(Type viewModelType)
    {
        NavigationEventArgs args = new() { ViewModelType = viewModelType };
        NavigationPerformed?.Invoke(this, args);
    }

    public event EventHandler<NavigationEventArgs> NavigationPerformed;
}
