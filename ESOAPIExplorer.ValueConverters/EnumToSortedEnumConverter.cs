using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.ValueConverters;

public class EnumToSortedEnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is List<string> enumvalues)
        {
            return enumvalues
                .Select(e => new EsoUIElement
                {
                    Name = e,
                    Value = ConstantValues.GetConstantValue(e),
                })
                .OrderBy(e => e.Value);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
