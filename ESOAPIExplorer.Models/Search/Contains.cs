using System;
using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search
{
    public class Contains : ISearchAlgorithm
    {
        public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets)
        {
            List<APIElement> results = [];

            foreach (APIElement target in targets)
            {
                if (target.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                {
                    results.Add(target);
                }
            }

            return results.Order();
        }
    }
}
