using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search;

public class DLDistance : ISearchAlgorithm
{
    public static string Name => "Damerau-Levenshtein Distance";

    public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
    {
        List<DLSearchResult> results = [];
        searchTerm = searchTerm.ToLower();

        foreach (APIElement target in targets)
        {
            int distance = Common.DamerauLevenshteinDistance(searchTerm, target.Name.ToLower());

            results.Add(new DLSearchResult { Target = target, Distance = distance });
        }

        return results
            .OrderBy(result => result.Distance)
            .Where(result => result.Distance < 5)
            .Select((result, index) =>
            {
                result.Target.Index = index;

                return result.Target;
            })
            .OrderBy(r => r.Index);
    }
}
