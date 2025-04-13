using ESOAPIExplorer.Services;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace ESOAPIExplorer;

/// <summary>
/// Provides application-specific behaviour to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private IConfigurationRoot _configurationRoot;
    private IConfigurationBuilder _configurationManager;

    public static Window MasterWindow { get; private set; }
    public IServiceProvider Container { get; private set; }
    public INavigationService Navigation {private set; get;}

    /// <summary>
    /// Initialises the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    /// 
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnStartup(StartupEventArgs args)
    {
        MasterWindow = new MainWindow();
        // MasterWindow.AppWindow.SetIcon("Assets/Images/win32Icon.ico");
        Container = RegisterDependencyInjection;

        Navigation = Container.GetRequiredService<INavigationService>();
        Navigation.InitializeAsync();
        MasterWindow.Activate();
    }

    private IServiceProvider RegisterDependencyInjection
    {
        get
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            services.AddSingleton<IConfiguration>(_configurationRoot);
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
        services.AddTransient<InfoViewModel>();
        services.AddTransient<ExportViewModel>();
    }

    private static void RegisterViews(ServiceCollection services)
    {
        services.AddSingleton<HomeView>();
        services.AddSingleton<ExportView>();
        services.AddSingleton<InfoView>();
        services.AddSingleton<SettingsView>();
    }

    private static void RegisterServices(ServiceCollection services)
    {
        //App Services
        services.AddSingleton(MasterWindow.Dispatcher);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventService, EventService>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddSingleton<IESODocumentationService, ESODocumentationService>();
        services.AddTransient<ILuaObjectScanner, LuaObjectScannerService>();
        services.AddSingleton<IRegexService, RegexService>();
        services.AddTransient<ILuaParserService, LuaParserService>();
        services.AddTransient<ILuaCheckRcGeneratorService, LuaCheckRcGeneratorService>();
        services.AddTransient<ILuaLanguageServerDefinitionsGeneratorService, LuaLanguageServerDefinitionsGeneratorService>();
        services.AddSingleton<ISettingsService, SettingsService>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        _configurationManager = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        _configurationRoot = _configurationManager.Build();
    }
}
