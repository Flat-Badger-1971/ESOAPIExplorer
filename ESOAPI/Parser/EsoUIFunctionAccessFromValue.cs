using System;
using System.Collections.Generic;

namespace ESOAPI.Parser;

public static class EsoUIFunctionAccessFromValue
{
    public static EsoUIFunctionAccess ToEsoUIFunctionAccess(this string access)
    {
        access = access.ToUpper().Replace("_", "-");

        return Enum.Parse<EsoUIFunctionAccess>(access);
    }
}
