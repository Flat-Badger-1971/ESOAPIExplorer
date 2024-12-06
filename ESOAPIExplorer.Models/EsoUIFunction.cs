using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIFunction
{
    private string _name;
    private EsoUIFunctionAccess _access;
    private List<EsoUIArgument> _args;
    private List<EsoUIArgument> _returns;
    private bool _variableReturns;
    private int _argumentTotal;

    public EsoUIFunction(string name, EsoUIFunctionAccess access = EsoUIFunctionAccess.PUBLIC)
    {
        _name = name;
        _access = access;
        _args = [];
        _returns = [];
        _variableReturns = false;
    }

    public void AddArgument(string name, string type = "")
    {
        _argumentTotal++;
        _args.Add(new EsoUIArgument(name, new EsoUIType(type), _argumentTotal));
    }

    public void AddReturn(string name, string type = "")
    {
        _returns.Add(new EsoUIArgument(name, new EsoUIType(type), _argumentTotal));
    }

    public string Name => _name;
    public EsoUIFunctionAccess Access => _access;

    public List<EsoUIArgument> Args
    {
        get => _args;
        set => _args = value;
    }

    public List<EsoUIArgument> Returns
    {
        get => _returns;
        set => _returns = value;
    }

    public bool HasVariableReturns
    {
        get => _variableReturns;
        set => _variableReturns = value;
    }
}
