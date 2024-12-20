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
            int distance = DamerauLevenshteinDistance(searchTerm, target.Name.ToLower());

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

    private static int DamerauLevenshteinDistance(string source, string target)
    {
        int[,] d = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j <= target.Length; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);

                if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1])
                {
                    d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                }
            }
        }

        return d[source.Length, target.Length];
    }
}
