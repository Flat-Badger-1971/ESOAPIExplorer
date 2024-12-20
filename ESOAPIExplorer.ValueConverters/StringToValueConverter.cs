using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class StringToValueConverter : IValueConverter
{
    public Type ReturnType { get; set; }
    public Dictionary<string, object> Values { get; set; } = [];

    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        string s = value.ToString();

        if (!string.IsNullOrWhiteSpace(s) && Values != null && Values.ContainsKey(s))
        {
            return Values[s];
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
