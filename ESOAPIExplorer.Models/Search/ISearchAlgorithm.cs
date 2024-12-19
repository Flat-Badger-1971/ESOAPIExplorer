using System.Collections.Generic;

namespace ESOAPIExplorer.Models.Search;

public interface ISearchAlgorithm
{
    public IEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets);
}
