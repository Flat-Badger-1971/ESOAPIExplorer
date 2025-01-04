using System;
using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class SIStringToVisibilityConverter : IValueConverter
{
    public object NullValue { get; set; }
    public object NotNullValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, string culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
