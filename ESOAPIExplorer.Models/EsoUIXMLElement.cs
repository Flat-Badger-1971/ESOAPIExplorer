using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIXMLElement
{
    public string Name { get; set; }
    public List<EsoUIArgument> Attributes { get; set; }
    public EsoUIType Parent { get; set; }
    public List<EsoUIType> Children { get; set; }
    public string Documentation { get; set; }

    public EsoUIXMLElement(string name)
    {
        Name = name;
        Attributes = [];
        Children = [];
    }
}
