using System;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class NullToValueConverter : IValueConverter
{
    public object NullValue { get; set; }
    public object NotNullValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value == null || (value is string && string.IsNullOrWhiteSpace(value.ToString())) || double.TryParse(value.ToString(), out _) && (double)value == 0)
        {
            return System.Convert.ChangeType(NullValue, ReturnType);
        }
        else
        {
            return System.Convert.ChangeType(NotNullValue, ReturnType);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
