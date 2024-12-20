using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search;

public class StartsWith : ISearchAlgorithm
{
    public static string Name => "Starts With";

    public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
    {
        List<APIElement> results = [];

        foreach (APIElement target in targets)
        {
            if (target.Name.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                results.Add(target);
            }
        }

        return results.Order();
    }
}

