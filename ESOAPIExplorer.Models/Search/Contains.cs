using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search;

public class Contains : ISearchAlgorithm
{
    public static string Name => "Contains";

    public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
    {
        return
            targets
            .Where(i =>
                string.IsNullOrWhiteSpace(searchTerm) || i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Name);
    }
}
