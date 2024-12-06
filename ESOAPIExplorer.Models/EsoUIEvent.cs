using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIEvent
{
    public string Name { get; set; }
    public List<EsoUIArgument> Args { get; set; }

    public EsoUIEvent(string name, List<EsoUIArgument> args)
    {
        Name = name;
        Args = args;
    }
}
