using System;
using ESOAPIExplorer.Models;
using System.Windows.Data;
using System.Globalization;

namespace ESOAPIExplorer.ValueConverters;

public class TypeToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EsoUIType arg)
        {
            return arg.Name;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
