using ESOAPIExplorer.Services;
using ESOAPIExplorer.ViewModels;
using ModernWpf;
using System;
using System.Windows;
using System.Windows.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ESOAPIExplorer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
#pragma warning disable CsWinRT1029 // Class not trimming / AOT compatible
public sealed partial class HomeView : Page
{
    public HomeView(ISettingsService settingsService)
    {
        string themeName = settingsService.GetSetting("ThemeName", "Dark");

        // change theme
        if (Enum.TryParse(themeName, out ApplicationTheme theme))
        {
            ThemeManager.Current.ApplicationTheme = theme;
        }

        InitializeComponent();

        INavigationService navigation = Application.Current.GetType().GetProperty("Navigation").GetValue(Application.Current) as INavigationService;
        ViewModelBase vm = (ViewModelBase)navigation.GetDataContextForPage(this);
        DataContext = vm;
        vm.InitializeAsync(null);

        ListViewGrid.SizeChanged += ListViewGridSizeChanged;
    }

    private void ListViewGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        HomeViewModel vm = (HomeViewModel)this.DataContext;
        if (vm != null)
        {
            vm.ListViewHeight = e.NewSize.Height;// sizeInfo.NewSize.Height;
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        //base.OnRenderSizeChanged(sizeInfo);
        //if (sizeInfo.NewSize.Width < 800)
        //{
        //    this.Width = 800;
        //}
        //else
        //{
        //    this.Width = sizeInfo.NewSize.Width;
        //}
    }

}
