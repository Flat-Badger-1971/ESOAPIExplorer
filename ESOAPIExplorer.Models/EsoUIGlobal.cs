using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class EsoUIGlobal
{
    public string Name { get; set; }
    public int? Value { get; set; }
    public string StringValue {  get; set; }
    public string ParentName { get; set; }
    public List<EsoUIElement> Parent { get; set; }
    public string Type { get; set; } = "integer";
    public APIElementType ElementType { get; set; } = APIElementType.GLOBAL;
}
