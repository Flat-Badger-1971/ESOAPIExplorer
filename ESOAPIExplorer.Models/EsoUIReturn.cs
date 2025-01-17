using System.Collections.Generic;

namespace ESOAPIExplorer.Models
{
    public class EsoUIReturn
    {
        public int Index { get; set; }
        public List<EsoUIArgument> Values { get; set; } = [];
    }
}
