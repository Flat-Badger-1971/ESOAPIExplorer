using ESOAPIExplorer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ESOAPIExplorer.ValueConverters;

public class ConstantToNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
