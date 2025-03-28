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
    }
}
