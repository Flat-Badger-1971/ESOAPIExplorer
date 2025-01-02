using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models;

public class EsoUIObject(string name)
{
    private IEnumerable<string> _FunctionList;

    public string Name { get; set; } = name;
    public ConcurrentDictionary<string, EsoUIFunction> Functions { get; set; } = [];
    public ICollection<string> Code { get; set; } = [];
    public string InstanceName { get; set; } = string.Empty;
    public IEnumerable<string> FunctionList {
        get
        {
#pragma warning disable IDE0305
            _FunctionList ??= Functions
                .Select(f => f.Value.Name)
                .Order()
                .ToList();

            return _FunctionList;
        }
    }

    public void AddFunction(EsoUIFunction data)
    {
        Functions[data.Name] = data;
    }

    public void AddInstanceName(string alias)
    {
        InstanceName = alias;
    }

    public void AddCode(string code)
    {
        if (!Functions.IsEmpty && code.StartsWith("* "))
        {
            Code.Add(string.Empty);
        }

        Code.Add(code);
    }
}
