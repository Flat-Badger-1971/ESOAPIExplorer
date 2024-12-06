using System;

namespace ESOAPIExplorer.EventArguments
{
    public class NavigationEventArgs:EventArgs
    {
        public Type ViewModelType { get; set; }
    }
}
