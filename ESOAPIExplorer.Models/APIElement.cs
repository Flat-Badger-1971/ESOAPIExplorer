using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models;

public class APIElement : IComparable<APIElement>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public APIElementType ElementType { get; set; }
    public string Parent { get; set; }
    public int Index { get; set; }
    public ICollection<string> Code { get; set; }

    public int CompareTo(APIElement obj) => string.Compare(Name, obj.Name);
}
