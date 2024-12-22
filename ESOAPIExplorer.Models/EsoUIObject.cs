using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIObject(string name)
{
    public string Name { get; set; } = name;
    public Dictionary<string, EsoUIFunction> Functions { get; set; } = [];
    public EsoUIObject Parent { get; set; }
    public List<EsoUIObject> Children { get; set; } = [];

    public void AddFunction(EsoUIFunction data)
    {
        Functions[data.Name] = data;
    }
}
