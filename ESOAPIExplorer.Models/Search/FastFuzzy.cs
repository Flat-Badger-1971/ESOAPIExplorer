using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search;

public class FastFuzzy : ISearchAlgorithm
{
    public IEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
    {
        List<FuzzySearchResult> results = [];
        searchTerm = searchTerm.ToLower();

        foreach (APIElement target in targets)
        {
            double score = CalculateScore(searchTerm, target.Name);

            if (score > double.NegativeInfinity)
            {
                results.Add(new FuzzySearchResult { Target = target, Score = score });
            }
        }

        return
            results
            .OrderByDescending(result => result.Score)
            .Select(result => result.Target);
    }

    private static double CalculateScore(string searchTerm, string target)
    {
        target = target.ToLower();
        int searchLen = searchTerm.Length;
        int targetLen = target.Length;
        int searchIndex = 0, targetIndex = 0;
        double score = 0;
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
