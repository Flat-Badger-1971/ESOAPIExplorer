using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class EventService(IDialogService dialogService) : IEventService
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly IDialogService _DialogService = dialogService;
#pragma warning restore IDE0052 // Remove unread private members

    public event EventHandler<bool> Busy;
    public event EventHandler DocumentationChanged;

    public Task RaiseBusy(bool isBusy)
    {
        Busy?.Invoke(this, isBusy);
        return Task.CompletedTask;
    }

    public Task RaiseDocumentationChanged()
    {
        DocumentationChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
