using System;

namespace ESOAPIExplorer.Models;

public class APIElement : IComparable<APIElement>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public APIElementType ElementType { get; set; }
    public string Parent { get; set; }
    public int Index { get; set; }

    public int CompareTo(APIElement obj)
    {
        return string.Compare(this.Name, obj.Name);
    }
}
