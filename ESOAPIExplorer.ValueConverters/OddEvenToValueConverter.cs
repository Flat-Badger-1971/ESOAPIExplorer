using System;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class OddEvenToValueConverter : IValueConverter
{
    public object EvenValue { get; set; }
    public object OddValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value is int num)
        {
            if (num % 2 == 0) //is even
                return EvenValue;
            else
                return OddValue;
        }

        else return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
