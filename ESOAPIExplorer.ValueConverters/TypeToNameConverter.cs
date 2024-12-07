using System;
using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class TypeToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value is EsoUIType arg)
        {
            return arg.Name;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
