using ESOAPIExplorer.Models;
using System.Windows;
using System.Windows.Controls;

namespace ESOAPIExplorer.ValueConverters;

public class TypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextBlockTemplate { get; set; }
    public DataTemplate ButtonTemplate { get; set; }

    //protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    //{
    //    if (item is EsoUIType type)
    //    {
    //        if (type.IsObject)
    //        {
    //            return ButtonTemplate;
    //        }

    //    }

    //    return TextBlockTemplate;
    //}
}
