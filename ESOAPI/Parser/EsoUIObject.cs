using System.Collections.Generic;

namespace ESOAPI.Parser;

public class EsoUIObject
{
    public string Name { get; set; }
    public Dictionary<string, EsoUIFunction> Functions { get; set; }
    public EsoUIObject Parent { get; set; }
    public List<EsoUIObject> Children { get; set; }

    public EsoUIObject(string name)
    {
        Name = name;
        Functions = [];
        Children = [];
    }

    public void AddFunction(EsoUIFunction data)
    {
        Functions[data.Name] = data;
    }
}
