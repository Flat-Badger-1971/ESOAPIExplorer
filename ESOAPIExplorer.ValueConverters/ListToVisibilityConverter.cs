using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class ListToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        Type t = value?.GetType();

        if (value == null)
        {
            return Visibility.Visible;
        }
        else if (value is IList list)
        {
            return list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if(value is IOrderedEnumerable<string> ienum)
        {
            return ienum.Any() ? Visibility.Collapsed : Visibility.Visible;
        }
        else if(value is IDictionary idic)
        {
            return idic.Values.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (t.GetProperty("ValueNames") != null)
        {
            List<string> valueList = (List<string>)t.GetProperty("ValueNames").GetValue(value);
            return valueList?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        else
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
