using ESOAPIExplorer.Models;
using System.Collections.Generic;

namespace ESOAPIExplorer.Services;

public interface ILuaFunctionScanner
{
    public string FolderPath { get; set; }

    public IDictionary<string, LuaFunctionDetails> ScanFolderForLuaFunctions();
}
