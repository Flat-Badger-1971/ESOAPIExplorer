using ESOAPIExplorer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ESOAPIExplorer.ValueConverters;

public class ElementToCodeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
