using System.Collections;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class LuaFunctionDetails
{
    public string Name { get; set; }
    public string Parameters { get; set; }
    public string ReturnType { get; set; }
    public IEnumerable<string> Code { get; set; }
}
