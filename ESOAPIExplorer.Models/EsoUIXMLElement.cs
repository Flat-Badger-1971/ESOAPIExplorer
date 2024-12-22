using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIXMLElement(string name)
{
    public string Name { get; set; } = name;
    public List<EsoUIArgument> Attributes { get; set; } = [];
    public EsoUIType Parent { get; set; }
    public List<EsoUIType> Children { get; set; } = [];
    public string Documentation { get; set; }
}
