using ESOAPIExplorer.Models;
using System.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace ESOAPIExplorer.ValueConverters
{
    public class EnumToSortedEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
