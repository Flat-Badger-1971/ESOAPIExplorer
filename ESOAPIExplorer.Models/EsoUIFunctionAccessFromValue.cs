using System;

namespace ESOAPIExplorer.Models;

public static class EsoUIFunctionAccessFromValue
{
    public static EsoUIFunctionAccess ToEsoUIFunctionAccess(this string access)
    {
        access = access.ToUpper().Replace("-", "_");

        return Enum.Parse<EsoUIFunctionAccess>(access);
    }
}
