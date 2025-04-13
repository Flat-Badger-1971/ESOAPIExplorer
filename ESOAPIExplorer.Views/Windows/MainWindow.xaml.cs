using ESOAPIExplorer.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ESOAPIExplorer.Views;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public Frame NavigationFrame;
    public Grid MainContainer;

    public MainWindow()
    {
        InitializeComponent();

        MainContainer = MainGrid;
        NavigationFrame = NavFrame;

        Loaded += (source, args) =>
        {
            IServiceProvider services = Application.Current.GetType().GetProperty("Container").GetValue(Application.Current) as IServiceProvider;
            MainViewModel vm = (MainViewModel)services.GetService(typeof(MainViewModel));
            HomeView home = (HomeView)services.GetService(typeof(HomeView));
            DataContext = vm;

            NavFrame.Navigate(home);
        };
    }
}
