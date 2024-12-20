using System.Collections.Generic;
using System.Linq;

namespace ESOAPIExplorer.Models.Search;

public interface ISearchAlgorithm
{
    public static string Name { get; }
    public IOrderedEnumerable<APIElement> Search(string searchTerm, IEnumerable<APIElement> targets);
}
