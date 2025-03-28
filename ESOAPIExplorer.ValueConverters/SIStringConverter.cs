using System;
using ESOAPIExplorer.Models;
using System.Windows.Data;
using System.Globalization;

namespace ESOAPIExplorer.ValueConverters;

public class SIStringToVisibilityConverter : IValueConverter
{
    public object NullValue { get; set; }
    public object NotNullValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EsoUIConstantValue constant && constant != null)
        {
            if (!string.IsNullOrEmpty(constant.StringValue))
            {
                return System.Convert.ChangeType(NotNullValue, ReturnType);
            }
        }

        return System.Convert.ChangeType(NullValue, ReturnType);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
