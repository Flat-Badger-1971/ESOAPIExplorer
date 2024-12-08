using System.Collections.Generic;

namespace ESOAPIExplorer.Models
{
    public class EsoUIEnum
    {
        public List<string> ValueNames { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> UsedBy { get; set; }
    }
}
