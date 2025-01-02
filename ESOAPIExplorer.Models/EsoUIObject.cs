using System.Collections.Concurrent;

namespace ESOAPIExplorer.Models;

public class EsoUIObject(string name)
{
    public string Name { get; set; } = name;
    public ConcurrentDictionary<string, EsoUIFunction> Functions { get; set; } = [];

    public string InstanceName { get; set; } = string.Empty;
    public void AddFunction(EsoUIFunction data)
    {
        Functions[data.Name] = data;
    }

    public void AddInstanceName(string alias)
    {
        InstanceName = alias;
    }
}
