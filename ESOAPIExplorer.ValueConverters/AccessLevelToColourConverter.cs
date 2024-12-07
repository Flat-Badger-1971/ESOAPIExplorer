using System;
using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Data;

namespace ESOAPIExplorer.ValueConverters;

public class AccessLevelToColourConverter : IValueConverter
{
    public object PublicValue { get; set; }
    public object PrivateValue { get; set; }
    public object ProtectedValue { get; set; }
    public object ProtectedAttributeValue { get; set; }
    public Type ReturnType { get; set; }

    public object Convert(object value, Type targetType, object parameter, string culture)
    {
        EsoUIFunctionAccess access = (EsoUIFunctionAccess)value; // Enum.Parse(typeof(EsoUIFunctionAccess), value.ToString().ToUpper());

        switch (access)
        {
            case EsoUIFunctionAccess.PRIVATE:
                return System.Convert.ChangeType(PrivateValue, ReturnType);
            case EsoUIFunctionAccess.PROTECTED:
                return System.Convert.ChangeType(ProtectedValue, ReturnType);
            case EsoUIFunctionAccess.PROTECTED_ATTRIBUTES:
                return System.Convert.ChangeType(ProtectedAttributeValue, ReturnType);
            case EsoUIFunctionAccess.PUBLIC:
                return System.Convert.ChangeType(PublicValue, ReturnType);
        }

        return System.Convert.ChangeType(PublicValue, ReturnType);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    {
        throw new NotImplementedException();
    }
}
