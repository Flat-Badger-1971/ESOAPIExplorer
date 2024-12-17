using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Models;

public static class DispatcherQueueExtensions
{
    public static Task EnqueueAsync(this DispatcherQueue dispatcherQueue, Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}
