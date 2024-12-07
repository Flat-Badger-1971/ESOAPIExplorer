using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIDocumentation
{
    public int ApiVersion { get; set; } = 0;
    public Dictionary<string, List<string>> Globals { get; set; } = [];
    public Dictionary<string, EsoUIFunction> Functions { get; set; } = [];
    public Dictionary<string, EsoUIObject> Objects { get; set; } = [];
    public Dictionary<string, EsoUIEvent> Events { get; set; } = [];
    public Dictionary<string, EsoUIArgument> XmlAttributes { get; set; } = [];
    public Dictionary<string, EsoUIXMLElement> XmlLayout { get; set; } = [];
}
