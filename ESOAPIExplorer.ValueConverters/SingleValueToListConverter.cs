using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.ValueConverters;

public class SingleValueToListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is EsoUIEvent esoevent)
        {
            return new List<EsoUIEvent> {  esoevent };
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
