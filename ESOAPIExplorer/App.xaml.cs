using ESOAPIExplorer.Services;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using Windows.ApplicationModel;

namespace ESOAPIExplorer;

/// <summary>
/// Provides application-specific behaviour to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static Window MainWindow { get; private set; }
    public IServiceProvider Container { get; private set; }
    private IConfigurationRoot _ConfigurationRoot;
    private IConfigurationBuilder _ConfigurationManager;
    /// <summary>
    /// Initialises the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.AppWindow.SetIcon("Assets/Images/win32Icon.ico");
        Container = RegisterDependencyInjection;

        INavigationService navigation = Container.GetRequiredService<INavigationService>();
        navigation.InitializeAsync();
        MainWindow.Activate();
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
        services.AddSingleton<HomeViewModel>();

        //Non View ViewModels
        services.AddTransient<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<CustomMessageDialogViewModel>();
    }

    private static void RegisterViews(ServiceCollection services)
    {
        services.AddTransient<HomeView>();
    }

    private void RegisterServices(ServiceCollection services)
    {
        //App Services
        services.AddSingleton(MainWindow.DispatcherQueue);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventService, EventService>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<IESODocumentationService, ESODocumentationService>();
        services.AddTransient<ILuaObjectScanner, LuaObjectScanneService>();
        services.AddSingleton<IRegexService, RegexService>();
        services.AddTransient<ILuaParserService, LuaParserService>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        _ConfigurationManager = new ConfigurationManager()
            .SetBasePath(Package.Current.InstalledLocation.Path)
            .AddJsonFile("appsettings.json", optional: false);

        _ConfigurationRoot = _ConfigurationManager.Build();
    }
}
