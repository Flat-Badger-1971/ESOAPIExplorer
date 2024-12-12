using ESOAPIExplorer.Services;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ESOAPIExplorer;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window m_window;
    public Window Window { get => m_window; }
    public IServiceProvider Container { get; private set; }
    private IConfigurationRoot _ConfigurationRoot;
    private IConfigurationBuilder _ConfigurationManager;
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        Container = RegisterDependencyInjection;

        INavigationService navigation = Container.GetRequiredService<INavigationService>();
        navigation.InitializeAsync();
        m_window.Activate();
    }


    private IServiceProvider RegisterDependencyInjection
    {
        get
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            services.AddSingleton<IConfiguration>(_ConfigurationRoot);
            RegisterServices(services);
            RegisterViews(services);
            RegisterViewModels(services);

            return services.BuildServiceProvider();
        }
    }

    private static void RegisterViewModels(ServiceCollection services)
    {
        services.AddTransient<HomeViewModel>();

        //Non View ViewModels
        services.AddTransient<MainViewModel>();
        services.AddSingleton<CustomMessageDialogViewModel>();
    }

    private static void RegisterViews(ServiceCollection services)
    {
        services.AddTransient<HomeView>();
    }

    private void RegisterServices(ServiceCollection services)
    {
        //App Services
        services.AddSingleton<DispatcherQueue>(Window.DispatcherQueue);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventService, EventService>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<IESODocumentationService, ESODocumentationService>();
    }
    private void ConfigureServices(IServiceCollection services)
    {
        _ConfigurationManager = new ConfigurationManager()
            .SetBasePath(Package.Current.InstalledLocation.Path)
            .AddJsonFile("appsettings.json", optional: false);

        _ConfigurationRoot = _ConfigurationManager.Build();
    }

}
