using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Models.Search;

public class FastFuzzy : ISearchAlgorithm
{
    public static string Name => "Fast Fuzzy";

    public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
    {
        ConcurrentBag<FuzzySearchResult> results = [];
        searchTerm = searchTerm.ToLower();

        Parallel.ForEach(targets, target =>
        {
            double score = CalculateScore(searchTerm, target.Name);

            if (score > double.NegativeInfinity)
            {
                results.Add(new FuzzySearchResult { Target = target, Score = score });
            }
        });

        //double maxScore = results.Max(r => r.Score);
        //double limit = maxScore - (maxScore / 3);

        return
            results
            // .Where(r => r.Score <= limit)
            .OrderByDescending(result => result.Score)
            .Select((result, index) =>
            {
                result.Target.Index = index;

                return result.Target;
            })
            .OrderBy(r => r.Index);
    }

    private static double CalculateScore(string searchTerm, string target)
    {
        target = target.ToLower();
        int searchLen = searchTerm.Length;
        int targetLen = target.Length;

        // Use Levenshtein Distance for a better score calculation
        double score = 1.0 / (1 + Common.LevenshteinDistance(searchTerm, target));

        int searchIndex = 0, targetIndex = 0;
        List<int> matchesSimple = [];

        while (searchIndex < searchLen && targetIndex < targetLen)
        {
            if (searchTerm[searchIndex] == target[targetIndex])
            {
                matchesSimple.Add(targetIndex);
                searchIndex++;
            }
            targetIndex++;
        }

        if (searchIndex != searchLen) { return double.NegativeInfinity; }

        // More complex scoring logic
        int unmatchedDistance = matchesSimple[matchesSimple.Count - 1] - matchesSimple[0] - (searchLen - 1);
        score -= (12 + unmatchedDistance) * (matchesSimple.Count - 1);

        if (matchesSimple[0] != 0)
        {
            score -= matchesSimple[0] * matchesSimple[0] * 0.2;
        }

        score -= (targetLen - searchLen) / 2;

        return score;
    }
}
