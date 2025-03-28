using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ESOAPIExplorer.Models;

public static class DispatcherQueueExtensions
{
    public static Task EnqueueAsync(this Dispatcher dispatcherQueue, Action action)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        dispatcherQueue.EnqueueAsync(() =>
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
