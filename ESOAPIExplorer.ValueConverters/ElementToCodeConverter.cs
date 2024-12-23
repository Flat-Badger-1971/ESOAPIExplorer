using System;
using System.Linq;
using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class ElementToCodeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        string code = string.Empty;

        if (value is APIElement element)
        {
            if (element.Code != null && element.Code.Count != 0)
            {
                code = string.Join("\n", element.Code);
            }
        }

        return code;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
