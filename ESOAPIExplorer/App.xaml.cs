using ESOAPIExplorer.Services;
using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;

namespace ESOAPIExplorer;

/// <summary>
/// Provides application-specific behaviour to supplement the default Application class.
/// </summary>
#pragma warning disable CA1416
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
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();

        try
        {
            MainWindow.AppWindow.SetIcon("Assets/Images/win32Icon.ico");
            Container = BuildServiceProvider();

            INavigationService navigation = Container.GetRequiredService<INavigationService>();
            await navigation.InitializeAsync();
        }
        catch (Exception ex)
        {
            ShowStartupFailure(ex);
        }
    }

    private IServiceProvider BuildServiceProvider()
    {
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        services.AddSingleton<IConfiguration>(_ConfigurationRoot);
        services.AddSingleton((MainWindow)MainWindow);
        services.AddSingleton<Window>(MainWindow);
        RegisterServices(services);
        RegisterViewModels(services);

        return services.BuildServiceProvider();
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

    private void RegisterServices(ServiceCollection services)
    {
        //App Services
        services.AddSingleton(MainWindow.DispatcherQueue);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IFilePickerService, PickerService>();
        services.AddSingleton<IFolderPickerService, PickerService>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddSingleton<IESODocumentationService, ESODocumentationService>();
        services.AddTransient<ILuaObjectScanner, LuaObjectScannerService>();
        services.AddSingleton<IRegexService, RegexService>();
        services.AddTransient<ILuaParserService, LuaParserService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddTransient<ILuaCheckRcGeneratorService, LuaCheckRcGeneratorService>();
        services.AddTransient<ILuaLanguageServerDefinitionsGeneratorService, LuaLanguageServerDefinitionsGeneratorService>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        _ConfigurationManager = new ConfigurationManager()
            .SetBasePath(Package.Current.InstalledLocation.Path)
            .AddJsonFile("appsettings.json", optional: false);

        _ConfigurationRoot = _ConfigurationManager.Build();
    }

    private static void ShowStartupFailure(Exception exception)
    {
        MainWindow.Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 12,
                Margin = new Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = "Application startup failed",
                        FontSize = 24,
                    },
                    new TextBlock
                    {
                        Text = exception.Message,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    new TextBox
                    {
                        Text = exception.ToString(),
                        IsReadOnly = true,
                        TextWrapping = TextWrapping.Wrap,
                        AcceptsReturn = true,
                        MinHeight = 240,
                    }
                }
            }
        };
    }
}
