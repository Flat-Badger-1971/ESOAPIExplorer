using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
        this.InitializeComponent();
        this.Activated += MainWindow_Activated;

        MainContainer = MainGrid;
        NavigationFrame = NavFrame;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.CodeActivated || args.WindowActivationState == WindowActivationState.PointerActivated)
        {
            SetTitleBarIcon();

            this.Activated -= MainWindow_Activated;
        }
    }

    private void SetTitleBarIcon()
    {
        nint hwnd = WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            string iconPath = "Assets/Logo.ico";
            appWindow.SetIcon(iconPath);

            AppWindowTitleBar titleBar = appWindow.TitleBar;

            if (titleBar != null)
            {
                titleBar.IconShowOptions = IconShowOptions.ShowIconAndSystemMenu;
            }
        }
    }
}
