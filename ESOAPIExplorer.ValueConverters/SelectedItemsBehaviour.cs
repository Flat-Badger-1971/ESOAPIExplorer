using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

namespace ESOAPIExplorer.ValueConverters;

public class SelectedItemsBehaviour : DependencyObject, IBehavior
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SelectedItemsBehaviour), new PropertyMetadata(null));

    public DependencyObject AssociatedObject { get; private set; }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public void Attach(DependencyObject associatedObject)
    {
        AssociatedObject = associatedObject;

        ListView listView = associatedObject as ListView;

        if (listView != null)
        {
            listView.SelectionChanged += ListView_SelectionChanged;
        }
    }

    public void Detach()
    {
        if (AssociatedObject != null)
        {
            ListView listView = AssociatedObject as ListView;

            if (listView != null)
            {
                listView.SelectionChanged -= ListView_SelectionChanged;
            }
        }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Command != null)
        {
            ListView listView = sender as ListView;

            if (listView != null)
            {
                Command.Execute(listView.SelectedItems);
            }
        }
    }
}
