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

        switch (true)
        {
            case true when value == null:
                return Visibility.Visible;
            case true when value is IList list:
                return list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            case true when value is IOrderedEnumerable<string> ienum:
                return ienum.Any() ? Visibility.Collapsed : Visibility.Visible;
            case true when value is IDictionary idic:
                return idic.Values.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            case true when t.GetProperty("ValueNames") != null:
                List<string> valueList = (List<string>)t.GetProperty("ValueNames").GetValue(value);

                return valueList?.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            default:
                return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
