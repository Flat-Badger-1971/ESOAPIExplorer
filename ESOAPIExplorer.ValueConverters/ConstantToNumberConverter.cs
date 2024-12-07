using System;
using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class ConstantToNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value is string constant)
        {
            return ConstantValues.GetConstantValue(constant);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
