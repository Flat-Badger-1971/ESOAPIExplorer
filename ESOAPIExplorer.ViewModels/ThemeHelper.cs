using Microsoft.UI.Xaml;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;
public static class ThemeHelper
{
    public static readonly DependencyProperty ActualThemeChangedCommandProperty = 
        DependencyProperty.RegisterAttached("ActualThemeChangedCommand", typeof(ICommand), typeof(ThemeHelper), new PropertyMetadata(null, OnActualThemeChangedCommandChanged));

    public static ICommand GetActualThemeChangedCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(ActualThemeChangedCommandProperty);
    }

    public static void SetActualThemeChangedCommand(DependencyObject obj, ICommand command)
    {
        obj.SetValue(ActualThemeChangedCommandProperty, command);
    }

    private static void OnActualThemeChangedCommandChanged(DependencyObject dobj, DependencyPropertyChangedEventArgs e)
    {
        if (dobj is FrameworkElement element)
        {
            element.ActualThemeChanged += (sender, args) =>
            {
                if (GetActualThemeChangedCommand(dobj) is ICommand command && command.CanExecute(element.ActualTheme))
                {
                    command.Execute(element.ActualTheme);
                }
            };
        }
    }
}
    