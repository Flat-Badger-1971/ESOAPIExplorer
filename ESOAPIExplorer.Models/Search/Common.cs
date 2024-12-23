using System;

namespace ESOAPIExplorer.Models.Search;

public static class Common
{
    public static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return string.IsNullOrEmpty(target) ? 0 : target.Length;
        if (string.IsNullOrEmpty(target)) return source.Length;

        int[] previousRow = new int[target.Length + 1];
        int[] currentRow = new int[target.Length + 1];

        for (int i = 0; i <= target.Length; i++)
            previousRow[i] = i;

        for (int i = 0; i < source.Length; i++)
        {
            currentRow[0] = i + 1;

            for (int j = 0; j < target.Length; j++)
            {
                int insertions = previousRow[j + 1] + 1;
                int deletions = currentRow[j] + 1;
                int substitutions = previousRow[j] + (source[i] == target[j] ? 0 : 1);

                currentRow[j + 1] = Math.Min(insertions, Math.Min(deletions, substitutions));
            }

            (currentRow, previousRow) = (previousRow, currentRow);
        }

        return previousRow[target.Length];
    }

    public static int DamerauLevenshteinDistance(string source, string target)
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
