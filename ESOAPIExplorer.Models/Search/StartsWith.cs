using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search
{
    internal class StartsWith : ISearchAlgorithm
    {
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
}

