using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ESOAPIExplorer.ValueConverters;

public class StringToValueConverter : IValueConverter
{
    public Type ReturnType { get; set; }
    public Dictionary<string, object> Values { get; set; } = [];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string s = value.ToString();

        if (!string.IsNullOrWhiteSpace(s) && Values != null && Values.TryGetValue(s, out object vout))
        {
            return vout;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
