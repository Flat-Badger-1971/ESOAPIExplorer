using System;
using System.Collections.Generic;

namespace ESOAPIExplorer.Models.Search
{
    public class Contains : ISearchAlgorithm
    {
        public IEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
        {
            List<APIElement> results = [];

            foreach (APIElement target in targets)
            {
                if (target.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                {
                    results.Add(target);
                }
            }

            return results;
        }
    }
}
