using ESOAPIExplorer.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Diagnostics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ESOAPIExplorer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
#pragma warning disable CsWinRT1029 // Class not trimming / AOT compatible
public sealed partial class HomeView : Page
#pragma warning restore CsWinRT1029 // Class not trimming / AOT compatible
{
    readonly Window _window =  (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);

    public HomeView()
    {
        this.InitializeComponent();

        //var hwnd = WindowNative.GetWindowHandle(this.XamlRoot.Content);
        //var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        
        //nint hwnd = Process.GetCurrentProcess().MainWindowHandle;
        //nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
        //WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        //AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
        

        //appWindow.SetIcon("Assets/Logo.ico");
    }

    private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox &&
            comboBox.SelectedItem is ComboBoxItem comboBoxItem &&
            comboBoxItem.Content is string themeString &&
            Enum.TryParse(themeString, out ElementTheme theme) is true)
        {
            this.RequestedTheme = theme;
        }
    }

    private void OnHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs e)
    {
        HomeViewModel context = (HomeViewModel)this.DataContext;

        context.SearchGithubCommand.Execute(this);
    }
}
