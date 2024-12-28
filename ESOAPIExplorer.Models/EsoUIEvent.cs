using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIEvent(string name, List<EsoUIArgument> args)
{
    public string Name { get; set; } = name;
    public List<EsoUIArgument> Args { get; set; } = args;
    public List<string> Code { get; set; } = [];

    public void AddCode(string line) => Code.Add(line);
}
