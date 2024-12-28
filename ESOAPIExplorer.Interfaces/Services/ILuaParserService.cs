using ESOAPIExplorer.Models;
using System.Collections.Generic;

namespace ESOAPIExplorer.Services
{
    public interface ILuaParserService
    {
        Dictionary<string, EsoUIGlobalValue> ParseLuaContent(string luaContent);
    }
}