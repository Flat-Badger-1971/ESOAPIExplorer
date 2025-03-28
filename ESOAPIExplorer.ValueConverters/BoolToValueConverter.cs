using System;
using System.Globalization;
using System.Windows.Data;

namespace ESOAPIExplorer.ValueConverters;

public class BoolToValueConverter : IValueConverter
{
    public object TrueValue { get; set; }
    public object FalseValue { get; set; }
    public object NullValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool v && v)
        {
            return System.Convert.ChangeType(TrueValue, ReturnType);
        }
        else if (value is null)
        {
            return System.Convert.ChangeType(NullValue, ReturnType);
        }
        else
        {
            return System.Convert.ChangeType(FalseValue, ReturnType);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
