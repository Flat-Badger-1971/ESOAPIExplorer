using ESOAPIExplorer.Models;

namespace ESOAPIExplorer.Services;

public interface ILuaObjectScanner
{
    public string FolderPath { get; set; }
    public LuaScanResults Results { get; set; }

    public void ScanFolderForLuaFunctions();
}
