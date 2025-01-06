using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIFunction(string name, EsoUIFunctionAccess access = EsoUIFunctionAccess.PUBLIC)
{
    private int _ArgumentTotal;
    private int _ReturnsTotal;

    public string Name { get; } = name;
    public EsoUIFunctionAccess Access { get; } = access;
    public List<EsoUIArgument> Args { get; set; } = [];
    public List<EsoUIArgument> Returns { get; set; } = [];
    public bool HasVariableReturns { get; set; } = false;
    public List<string> Code { get; set; } = [];
    public string Parent { get; set; }
    public void AddArgument(string name, string type = "Unknown") => Args.Add(new EsoUIArgument(name, new EsoUIType(type), ++_ArgumentTotal));
    public void AddReturn(string name, string type = "Unknown") => Returns.Add(new EsoUIArgument(name, new EsoUIType(type), ++_ReturnsTotal));
    public void AddCode(string line) => Code.Add(line);
}
