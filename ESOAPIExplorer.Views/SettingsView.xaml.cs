using System.Windows.Controls;

namespace ESOAPIExplorer.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
#pragma warning disable CsWinRT1029 // Class not trimming / AOT compatible
public sealed partial class SettingsView : Page
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
