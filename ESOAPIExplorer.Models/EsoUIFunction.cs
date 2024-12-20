using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIFunction
{
    private int _ArgumentTotal;

    public string Name { get; }
    public EsoUIFunctionAccess Access { get; }
    public List<EsoUIArgument> Args { get; set; }
    public List<EsoUIArgument> Returns { get; set; }
    public bool HasVariableReturns { get; set; }

    public EsoUIFunction(string name, EsoUIFunctionAccess access = EsoUIFunctionAccess.PUBLIC)
    {
        Name = name;
        Access = access;
        Args = [];
        Returns = [];
        HasVariableReturns = false;
    }

    public void AddArgument(string name, string type = "")
    {
        _ArgumentTotal++;
        Args.Add(new EsoUIArgument(name, new EsoUIType(type), _ArgumentTotal));
    }

    public void AddReturn(string name, string type = "")
    {
        Returns.Add(new EsoUIArgument(name, new EsoUIType(type), _ArgumentTotal));
    }
}
