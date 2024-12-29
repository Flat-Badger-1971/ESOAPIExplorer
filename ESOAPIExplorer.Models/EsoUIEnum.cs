using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIEnum
{
    public IEnumerable<EsoUIEnumValue> Values { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> UsedBy { get; set; }
}
