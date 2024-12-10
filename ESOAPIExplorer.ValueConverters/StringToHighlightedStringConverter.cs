using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace ESOAPIExplorer.ValueConverters;

public class StringToHighlightedStringConverter : DependencyObject, IValueConverter
{
    public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register(nameof(Parameter), typeof(string), typeof(StringToHighlightedStringConverter), null);


    public string Parameter
    {
        get => (string)GetValue(ParameterProperty);
        set
        {
            SetValue(ParameterProperty, value);
        }
    }


    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        if (value == null)
            return string.Empty;
        else 
        {
            if (string.IsNullOrEmpty(Parameter))
            {
                return string.Empty;
            }

            string highlightedChars = string.Empty;

            bool print = false;
            foreach (char ch in value.ToString())
            {
                print = !print;
                highlightedChars += Parameter.ToLower().Contains(ch.ToString().ToLower()) ? ch : " ";
            }
            //return value.ToString().ToList();
            return highlightedChars;
            
        }
    }


    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
