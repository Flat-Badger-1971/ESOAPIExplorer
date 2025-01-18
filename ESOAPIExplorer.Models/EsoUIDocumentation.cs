using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIDocumentation
{
    public int ApiVersion { get; set; } = 0;
    public ConcurrentDictionary<string, EsoUIConstantValue> Constants { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIEvent> Events { get; set; } = [];
    public ConcurrentDictionary<string, bool> Fragments { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIFunction> Functions { get; set; } = [];
    public ConcurrentDictionary<string, ICollection<EsoUIEnumValue>> Globals { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIInstance> InstanceNames { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIObject> Objects { get; set; } = [];
    public Dictionary<string, string> SI_Lookup { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIArgument> XmlAttributes { get; set; } = [];
    public ConcurrentDictionary<string, EsoUIXMLElement> XmlLayout { get; set; } = [];
}
