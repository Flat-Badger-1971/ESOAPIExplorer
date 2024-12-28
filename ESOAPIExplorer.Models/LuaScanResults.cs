using System.Collections.Concurrent;

namespace ESOAPIExplorer.Models
{
    public class LuaScanResults
    {
        public ConcurrentBag<EsoUIFunction> Functions { get; set; } = [];
        public ConcurrentBag<EsoUIGlobal> Globals { get; set; } = [];
        public ConcurrentBag<EsoUIObject> Objects { get; set; } = [];
    }
}
