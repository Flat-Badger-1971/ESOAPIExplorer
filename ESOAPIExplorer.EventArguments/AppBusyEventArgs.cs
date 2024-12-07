using System;

namespace ESOAPIExplorer.EventArguments;

public class AppBusyEventArgs : EventArgs
{
    public bool IsBusy { get; set; }
}
