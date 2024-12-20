using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IEventService
{
    public event EventHandler<bool> Busy;
    public Task RaiseBusy(bool isBusy);
}
