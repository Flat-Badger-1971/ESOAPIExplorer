using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.ValueConverters
{
    public class EnumToSortedEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<EsoUIEnumValue> enumvalues)
            {
                int count = enumvalues.Count;
                List<EsoUIElement> esoUIElements = new List<EsoUIElement>(count);

                return enumvalues
                    .Select(e =>
                    {
                        if (!int.TryParse(e.Value?.ToString(), out int order))
                        {
                            order = 0;
                        };

                        return new EsoUIElement
                        {
                            Name = e.Name,
                            Value = e.Value?.ToString(),
                            Type = e.Type,
                            Order = order
                        };
                    })
                    .OrderBy(e => e.Order);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
