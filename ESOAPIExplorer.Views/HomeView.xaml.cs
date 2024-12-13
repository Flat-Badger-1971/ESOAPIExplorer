using ESOAPIExplorer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;

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
    public HomeView()
    {
        this.InitializeComponent();
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
