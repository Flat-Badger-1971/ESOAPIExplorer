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
            EsoUIGlobalValue globalvalue = ConstantValues.GetConstantValue(constant);

            if (globalvalue.IntValue.HasValue)
            {
                return globalvalue.IntValue.ToString();
            }

            if (globalvalue.DoubleValue.HasValue)
            {
                return globalvalue.DoubleValue.ToString();
            }

            return globalvalue.StringValue;

        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
