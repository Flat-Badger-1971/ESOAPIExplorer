using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services
{
    public interface IEventService
    {
        event EventHandler<bool> Busy;
        Task RaiseBusy(bool isBusy);
    }
}
