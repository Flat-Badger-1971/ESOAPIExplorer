using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models;

public class EsoUIFunction(string name, EsoUIFunctionAccess access = EsoUIFunctionAccess.PUBLIC)
{
    private int _ArgumentTotal;

    public string Name { get; } = name;
    public EsoUIFunctionAccess Access { get; } = access;
    public List<EsoUIArgument> Args { get; set; } = [];
    public List<EsoUIReturn> Returns { get; set; } = [];
    public bool HasVariableReturns { get; set; } = false;
    public List<string> Code { get; set; } = [];
    public string Parent { get; set; }
    public void AddArgument(string name, string type = "any") => Args.Add(new EsoUIArgument(name, new EsoUIType(type), ++_ArgumentTotal));
    public void AddReturns(List<EsoUIReturn> returns) => Returns.AddRange(returns);
    public void AddReturn(EsoUIReturn ret) => Returns.Add(ret);
    public void AddReturn(string name, string type = "any")
    {
        EsoUIReturn ret = new EsoUIReturn();
        ret.Values.Add(new EsoUIArgument(name, new EsoUIType(type), 1));
        Returns.Add(ret);
    }
    public void AddCode(string line) => Code.Add(line);
    public APIElementType ElementType { get; set; } = APIElementType.C_FUNCTION;
    public void SetReturn(string value)
    {
        Returns.Clear();
    }
    public void SetArgument(string name, string type)
    {
        Args.Clear();
        _ArgumentTotal = 0;
        AddArgument(name, type);
    }
}
