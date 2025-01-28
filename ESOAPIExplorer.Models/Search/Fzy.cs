using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Fzy search algorithm converted from a lua implementation of the original C code.
// Original code: https://github.com/swarn/fzy-lua/blob/main/docs/fzy.md
// Lua implementation: https://github.com/davidoakley/ESO-Navigator/blob/main/fzy.lua

namespace ESOAPIExplorer.Models.Search
{
    public class Fzy : ISearchAlgorithm
    {
        public static string Name => "Fzy";

        private record PositionResult(int[] Positions, double Score);
        private record FilterResult(int Index, int[] Positions, double Score);

        private const double SCORE_GAP_LEADING = -0.005;
        private const double SCORE_GAP_TRAILING = -0.005;
        private const double SCORE_GAP_INNER = -0.01;
        private const double SCORE_MATCH_CONSECUTIVE = 1.0;
        private const double SCORE_MATCH_SLASH = 0.9;
        private const double SCORE_MATCH_WORD = 0.8;
        private const double SCORE_MATCH_CAPITAL = 0.7;
        private const double SCORE_MATCH_DOT = 0.6;
        private const double SCORE_MAX = double.PositiveInfinity;
        private const double SCORE_MIN = double.NegativeInfinity;
        private const int MATCH_MAX_LENGTH = 1024;

        /// <summary>
        /// Searches for the specified search term in the given targets.
        /// </summary>
        /// <param name="searchTerm">The search term to look for.</param>
        /// <param name="targets">The collection of API elements to search in.</param>
        /// <returns>An ordered enumerable of API elements that match the search term.</returns>
        public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
        {
            ConcurrentBag<FuzzySearchResult> results = [];

            Parallel.ForEach(targets, target =>
            {
                if (HasMatch(searchTerm, target.Name))
                {
                    double score = Score(searchTerm, target.Name);
                    results.Add(new FuzzySearchResult { Target = target, Score = score });
                }
            });

            return results
                .OrderByDescending(result => result.Score)
                .Select((result, index) =>
                {
                    result.Target.Index = index;
                    return result.Target;
                })
                .OrderBy(r => r.Index);
        }

        /// <summary>
        /// Checks if the needle is a subsequence of the haystack.
        /// </summary>
        /// <param name="needle">The string to search for.</param>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive.</param>
        /// <returns>True if the needle is a subsequence of the haystack, otherwise false.</returns>
        private static bool HasMatch(string needle, string haystack, bool caseSensitive = false)
        {
            if (!caseSensitive)
            {
                needle = needle.ToLower();
                haystack = haystack.ToLower();
            }

            int j = 0;

            for (int i = 0; i < needle.Length; i++)
            {
                j = haystack.IndexOf(needle[i], j);

                if (j == -1)
                {
                    return false;
                }

                j++;
            }

            return true;
        }

        /// <summary>
        /// Precomputes match bonuses for the haystack.
        /// </summary>
        /// <param name="haystack">The string to precompute bonuses for.</param>
        /// <returns>An array of match bonuses.</returns>
        private static double[] PrecomputeBonus(string haystack)
        {
            double[] matchBonus = new double[haystack.Length];
            char lastChar = '/';

            for (int i = 0; i < haystack.Length; i++)
            {
                char thisChar = haystack[i];

                switch (true)
                {
                    case true when lastChar == '/' || lastChar == '\\':
                        matchBonus[i] = SCORE_MATCH_SLASH;
                        break;
                    case true when lastChar == '-' || lastChar == '_' || lastChar == ' ':
                        matchBonus[i] = SCORE_MATCH_WORD;
                        break;
                    case true when lastChar == '.':
                        matchBonus[i] = SCORE_MATCH_DOT;
                        break;
                    case true when char.IsLower(lastChar) && char.IsUpper(thisChar):
                        matchBonus[i] = SCORE_MATCH_CAPITAL;
                        break;
                    default:
                        matchBonus[i] = 0;
                        break;
                }

                lastChar = thisChar;
            }

            return matchBonus;
        }

        /// <summary>
        /// Computes the dynamic programming tables D and M for scoring.
        /// </summary>
        /// <param name="needle">The string to search for.</param>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="D">The dynamic programming table D.</param>
        /// <param name="M">The dynamic programming table M.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive.</param>
        private static void Compute(string needle, string haystack, double[][] D, double[][] M, bool caseSensitive)
        {
            double[] matchBonus = PrecomputeBonus(haystack);
            int n = needle.Length;
            int m = haystack.Length;

            if (!caseSensitive)
            {
                needle = needle.ToLower();
                haystack = haystack.ToLower();
            }

            char[] haystackChars = haystack.ToCharArray();

            for (int i = 0; i < n; i++)
            {
                D[i] = new double[m];
                M[i] = new double[m];

                double prevScore = SCORE_MIN;
                double gapScore = i == n - 1 ? SCORE_GAP_TRAILING : SCORE_GAP_INNER;
                char needleChar = needle[i];

                for (int j = 0; j < m; j++)
                {
                    if (needleChar == haystackChars[j])
                    {
                        double score = SCORE_MIN;

                        if (i == 0)
                        {
                            score = (j * SCORE_GAP_LEADING) + matchBonus[j];
                        }
                        else if (j > 0)
                        {
                            double a = M[i - 1][j - 1] + matchBonus[j];
                            double b = D[i - 1][j - 1] + SCORE_MATCH_CONSECUTIVE;
                            score = Math.Max(a, b);
                        }

                        D[i][j] = score;
                        prevScore = Math.Max(score, prevScore + gapScore);
                        M[i][j] = prevScore;
                    }
                    else
                    {
                        D[i][j] = SCORE_MIN;
                        prevScore += gapScore;
                        M[i][j] = prevScore;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the matching score for the needle in the haystack.
        /// </summary>
        /// <param name="needle">The string to search for.</param>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive.</param>
        /// <returns>The matching score.</returns>
        private static double Score(string needle, string haystack, bool caseSensitive = false)
        {
            int n = needle.Length;
            int m = haystack.Length;

            if (n == 0 || m == 0 || m > MATCH_MAX_LENGTH || n > m)
            {
                return SCORE_MIN;
            }
            else if (n == m)
            {
                return SCORE_MAX;
            }
            else
            {
                double[][] D = new double[n][];
                double[][] M = new double[n][];
                Compute(needle, haystack, D, M, caseSensitive);

                return M[n - 1][m - 1];
            }
        }

        /// <summary>
        /// Computes the positions where the needle matches the haystack.
        /// </summary>
        /// <param name="needle">The string to search for.</param>
        /// <param name="haystack">The string to search in.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive.</param>
        /// <returns>A PositionResult containing the positions and score.</returns>
        private static PositionResult Positions(string needle, string haystack, bool caseSensitive = false)
        {
            int n = needle.Length;
            int m = haystack.Length;

            if (n == 0 || m == 0 || m > MATCH_MAX_LENGTH || n > m)
            {
                return new PositionResult([], SCORE_MIN);
            }
            else if (n == m)
            {
                int[] consecutive = Enumerable.Range(1, n).ToArray();

                return new PositionResult(consecutive, SCORE_MAX);
            }

            double[][] D = new double[n][];
            double[][] M = new double[n][];
            Compute(needle, haystack, D, M, caseSensitive);

            int[] positions = new int[n];
            bool matchRequired = false;
            int j = m - 1;

            for (int i = n - 1; i >= 0; i--)
            {
                while (j >= 0)
                {
                    if (D[i][j] != SCORE_MIN && (matchRequired || D[i][j] == M[i][j]))
                    {
                        matchRequired = (i != 0) && (j != 0) && (M[i][j] == D[i - 1][j - 1] + SCORE_MATCH_CONSECUTIVE);
                        positions[i] = j + 1;
                        j--;
                        break;
                    }
                    else
                    {
                        j--;
                    }
                }
            }

            return new PositionResult(positions, M[n - 1][m - 1]);
        }

        /// <summary>
        /// Applies HasMatch and Positions to an array of haystacks.
        /// </summary>
        /// <param name="needle">The string to search for.</param>
        /// <param name="haystacks">The list of strings to search in.</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive.</param>
        /// <returns>A list of FilterResult containing the index, positions, and score for each matching line.</returns>
        private static List<FilterResult> Filter(string needle, List<string> haystacks, bool caseSensitive = false)
        {
            List<FilterResult> result = [];

            Parallel.For(0, haystacks.Count, i =>
            {
                string line = haystacks[i];

                if (HasMatch(needle, line, caseSensitive))
                {
                    PositionResult positionsResult = Positions(needle, line, caseSensitive);
                    result.Add(new FilterResult(i, positionsResult.Positions, positionsResult.Score));
                }
            });

            return result;
        }
    }
}
