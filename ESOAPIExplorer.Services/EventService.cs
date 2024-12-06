using ESOAPIExplorer.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Sainsburys.LDM.CubiScanKiosk.Services
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public class EventService(IDialogService dialogService) : IEventService
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IDialogService _DialogService = dialogService;
#pragma warning restore IDE0052 // Remove unread private members

        public event EventHandler<bool> Busy;

        public Task RaiseBusy(bool isBusy)
        {
            Busy?.Invoke(this, isBusy);
            return Task.CompletedTask;
        }
    }
}
