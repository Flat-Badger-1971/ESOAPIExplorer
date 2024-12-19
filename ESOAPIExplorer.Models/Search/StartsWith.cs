using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models.Search
{
    internal class StartsWith : ISearchAlgorithm
    {
        public IEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
        {
            List<APIElement> results = [];

            foreach (APIElement target in targets)
            {
                if (target.Name.StartsWith(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                {
                    results.Add(target);
                }
            }

            return results;
        }
    }
}

