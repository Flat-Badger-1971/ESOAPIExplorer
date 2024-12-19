using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search
{
    public class SymSpell : ISearchAlgorithm
    {
        private readonly Dictionary<string, List<string>> _dictionary = [];
        private const int MAX_EDIT_DISTANCE = 2;

        public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
        {
            BuildDictionary(targets);
            searchTerm = searchTerm.ToLower();
            List<APIElement> results = [];

            foreach (string key in _dictionary.Keys)
            {
                if (DamerauLevenshteinDistance(searchTerm, key) <= MAX_EDIT_DISTANCE)
                {
                    results.AddRange(_dictionary[key].Select(name => new APIElement { Name = name }));
                }
            }

            return results.Order();
        }

        private void BuildDictionary(IEnumerable<APIElement> targets)
        {
            _dictionary.Clear();

            foreach (APIElement target in targets)
            {
                string key = CreateDictionaryKey(target.Name.ToLower());

                if (!_dictionary.TryGetValue(key, out List<string> value))
                {
                    value = [];
                    _dictionary[key] = value;
                }

                value.Add(target.Name);
            }
        }

        private static string CreateDictionaryKey(string word)
        {
            char[] key = new char[word.Length];

            for (int i = 0; i < word.Length; i++)
            {
                key[i] = word[i];
            }

            Array.Sort(key);

            return new string(key);
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
}
