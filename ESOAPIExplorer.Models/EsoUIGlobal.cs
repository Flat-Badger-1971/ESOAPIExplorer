using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIGlobal
{
    public string Name { get; set; }
    public int? Value { get; set; }
    public string ParentName { get; set; }
    public List<EsoUIElement> Parent { get; set; }
}
